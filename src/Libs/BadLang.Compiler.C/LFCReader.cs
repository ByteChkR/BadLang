using System.Text;

using LF.Compiler.C.Expressions;
using LF.Compiler.C.Expressions.Access;
using LF.Compiler.C.Expressions.Branches;
using LF.Compiler.C.Expressions.ControlFlow;
using LF.Compiler.C.Expressions.Internal;
using LF.Compiler.C.Expressions.Loops;
using LF.Compiler.C.Expressions.Value;
using LF.Compiler.C.Functions;
using LF.Compiler.C.Operators;
using LF.Compiler.C.Types;
using LF.Compiler.DataObjects;
using LF.Compiler.DataObjects.Code.Instruction;
using LF.Compiler.Logging;
using LF.Compiler.Reader;

namespace LF.Compiler.C;

public class LFCReader : LFCodeReader
{
    private readonly LFReaderCodeSection? m_DefaultSection;

    public LFCReader(string source, string workingDirectory, string file, LFReaderCodeSection? defaultSection = null) : base(source, workingDirectory, file)
    {
        m_DefaultSection = defaultSection;
    }


    public void SkipNonToken()
    {
        while (!IsEOF())
        {
            if (Is('/') && Is('/', 1))
            {
                SkipToNewLine();
            }
            else if (IsWhiteSpace())
            {
                SkipWhiteSpace();
            }
            else
            {
                break;
            }
        }
    }

    public LFCTypeToken ReadType(bool allowWordStart)
    {
        string readWord = ReadWord();
        List<LFCTypeToken> typeArgs = new List<LFCTypeToken>();
        SkipNonToken();
        int start = CurrentIndex;
        try
        {
            if (Is('<'))
            {
                Eat('<');
                SkipNonToken();
                while (!Is('>'))
                {
                    typeArgs.Add(ReadType(false));
                    SkipNonToken();
                    if (Is('>'))
                    {
                        break;
                    }

                    Eat(',');
                    SkipNonToken();
                }

                Eat('>');
            }
        }
        catch (Exception)
        {
            //Logger.Error(e);
            typeArgs.Clear();
            SetIndex(start);
        }

        if (Is('('))
        {
            typeArgs.Clear();
            SetIndex(start);
        }

        SkipNonToken();
        int pointerLevel = 0;
        int pointerLevelStart = CurrentIndex;
        while (Is('*'))
        {
            pointerLevel++;
            readWord += '*';
            SkipNonToken();
            Eat('*');
            SkipNonToken();
        }

        SkipNonToken();

        if (!allowWordStart && (IsWordStart() || IsDigit()))
        {
            readWord = readWord.Remove(readWord.Length - pointerLevel, pointerLevel);
            SetIndex(pointerLevelStart);
        }

        return new LFCTypeToken(readWord, typeArgs.ToArray());
    }

    private void EmitEntryCode(LFReaderResult result, string entryPoint)
    {
        LFReaderCodeSection codeSection = result.GetOrCreateCodeSection("entry");
        codeSection.CreateLabel("__entry__", LFSourcePosition.Unknown);
        codeSection.AddImport(result.Name);
        codeSection.Emit(new LFAssemblyInstruction(LFSourcePosition.Unknown, OpCodes.Push64, $"{result.Name}::{entryPoint}"));
        codeSection.Emit(new LFAssemblyInstruction(LFSourcePosition.Unknown, OpCodes.Call));
        codeSection.Emit(new LFAssemblyInstruction(LFSourcePosition.Unknown, OpCodes.Halt));
    }

    private LFCExpression CreateStringValue(LFReaderResult result, LFReaderCodeSection currentSection, string rName, string text, LFSourcePosition position)
    {
        LFCTypeDatabase db = LFCTypeDatabase.GetTypeSystem(result);
        LFReaderDataSection data = result.GetOrCreateDataSection($"{result.Name}_data");

        
        string vName = data.EmitData(Encoding.UTF8.GetBytes(text));
        string name = $"{data.Name}::{vName}";

        currentSection.AddImport(data.Name);

        return new LFCVariableExpression(name, position, db.GetType("i8*", position));
    }

    public LFCExpression ReadValue(LFReaderResult result, LFReaderCodeSection codeSection, int precedence)
    {
        SkipNonToken();

        LFSourcePosition symPos = CreatePosition();
        string sym = ReadUntilWordOrWhiteSpace();
        symPos = symPos.Combine(CreatePosition());

        if (LFCOperatorTable.HasPreValueOperator(sym, precedence))
        {
            LFCPreValueOperator op = LFCOperatorTable.GetPreValueOperator(sym, precedence, symPos);

            return op.Parse(new LFCPreValueOperatorParseInput(codeSection, this, result));
        }

        MovePrevious(sym.Length);


        if (IsDigit())
        {
            LFSourcePosition position = CreatePosition();
            ulong num = ReadNumber();
            position = position.Combine(CreatePosition());

            return new LFCValueExpression(num, position);
        }

        if (Is('"'))
        {
            LFSourcePosition position = CreatePosition();
            string readString = $"{ReadString()}\0";
            position = position.Combine(CreatePosition());

            return CreateStringValue(result, codeSection, "__LFC__~CSTRING~", readString, position);
        }

        if (IsWordStart())
        {
            LFSourcePosition wordStart = CreatePosition();
            LFCTypeToken word = ReadType(false);
            wordStart = wordStart.Combine(CreatePosition());

            if (LFCParser.HasCompilerFunction(word.Name))
            {
                int start = CurrentIndex;
                if (!TryReadInvocationArguments(result, codeSection, out (LFCTypeToken[], LFCExpression[]) output))
                {
                    SetIndex(start);
                }
                else
                {
                    return new LFCCompilerFunctionExpression(word.Name, output.Item2, output.Item1, wordStart.Combine(CreatePosition()));
                }
            }

            if (word.Name == "return")
            {
                SkipNonToken();

                if (Is(';'))
                {
                    return new LFCReturnExpression(wordStart);
                }

                return new LFCReturnExpression(wordStart, ReadExpression(result, codeSection));
            }
            if (word.Name == "break")
            {
                SkipNonToken();
                return new LFCBreakExpression(wordStart);
            }
            if (word.Name == "continue")
            {
                SkipNonToken();
                return new LFCContinueExpression(wordStart);
            }

            if (word.Name == "if")
            {
                return ParseIf(result, codeSection, wordStart);
            }

            if (word.Name == "new")
            {
                SkipNonToken();
                LFCTypeToken type = ReadType(false);
                if (!TryReadInvocationArguments(result, codeSection, out (LFCTypeToken[], LFCExpression[]) output))
                {
                    throw new LFCParserException("Invalid arguments for new", wordStart);
                }

                return new LFCNewExpression(wordStart, new LFCTypeToken(type.Name, output.Item1), output.Item2);
            }

            if (word.Name == "for")
            {
                SkipNonToken();
                Eat('(');
                SkipNonToken();
                LFCExpression? init = null;
                if (!Is(';'))
                {
                    init = ReadExpression(result, codeSection);
                }

                SkipNonToken();
                Eat(';');
                SkipNonToken();
                LFCExpression? cond = null;
                if (!Is(';'))
                {
                    cond = ReadExpression(result, codeSection);
                }

                SkipNonToken();
                Eat(';');
                SkipNonToken();
                LFCExpression? inc = null;
                if (!Is(')'))
                {
                    inc = ReadExpression(result, codeSection);
                }

                SkipNonToken();
                Eat(')');
                SkipNonToken();
                Eat('{');
                SkipNonToken();
                List<LFCExpression> body = ReadBlock(result, codeSection);

                return new LFCForExpression(wordStart, init, cond, inc, body.ToArray());
            }

            if (word.Name == "while")
            {
                SkipNonToken();
                Eat('(');
                SkipNonToken();
                LFCExpression condition = ReadExpression(result, codeSection);
                SkipNonToken();
                Eat(')');
                SkipNonToken();
                Eat('{');
                SkipNonToken();
                List<LFCExpression> body = ReadBlock(result, codeSection);

                return new LFCWhileExpression(wordStart, condition, body.ToArray());
            }

            if (word.Name == "switch")
            {
                SkipNonToken();
                Eat('(');
                SkipNonToken();
                LFCExpression value = ReadExpression(result, codeSection);
                SkipNonToken();
                Eat(')');

                LFSourcePosition switchPos = wordStart.Combine(CreatePosition());
                SkipNonToken();
                Eat('{');
                SkipNonToken();
                Dictionary<LFCExpression, LFCExpression[]> caseLabels = new Dictionary<LFCExpression, LFCExpression[]>();
                LFCExpression[]? defaultCase = null;
                while (!Is('}'))
                {
                    
                    SkipNonToken();
                    LFSourcePosition caseLabelPos = CreatePosition();
                    if (defaultCase != null)
                    {
                        throw new LFCParserException("Expected Default Case to be the last case in the switch statement", caseLabelPos);
                    }
                    string caseLabel = ReadWord();
                    SkipNonToken();
                    if (caseLabel == "case")
                    {
                        LFCExpression caseValue = ReadExpression(result, codeSection);
                        SkipNonToken();
                        Eat(':');
                        SkipNonToken();
                        List<LFCExpression> caseBlock = new List<LFCExpression>();
                        while (!Is("case") && !Is("default") && !Is('}'))
                        {
                            caseBlock.Add(ReadExpression(result, codeSection));
                            SkipNonToken();
                            Eat(';');
                            SkipNonToken();
                        }
                        
                        caseLabels.Add(caseValue, caseBlock.ToArray());
                        
                    }
                    else if (caseLabel == "default")
                    {
                        SkipNonToken();
                        Eat(':');
                        SkipNonToken();
                        
                        List<LFCExpression> caseBlock = new List<LFCExpression>();
                        while (!Is("case") && !Is("default") && !Is('}'))
                        {
                            caseBlock.Add(ReadExpression(result, codeSection));
                            SkipNonToken();
                            Eat(';');
                            SkipNonToken();
                        }

                        defaultCase = caseBlock.ToArray();
                    }
                    else
                    {
                        throw new LFCParserException("Invalid Switch/Case Label", caseLabelPos);
                    }
                    SkipNonToken();
                }
                
                //Eat('}');

                return new LFCSwitchExpression(switchPos, value, caseLabels, defaultCase);
            }

            LFCPreProcessor pp = LFCPreProcessor.GetPreProcessor(result);
            if (pp.IsDefined(word.Name))
            {
                return CreateStringValue(result, codeSection, "__LFC__PP__~DEFINE~", $"{pp.GetDefine(word.Name)}\0", wordStart);
            }

            SkipNonToken();
            int prePointer = CurrentIndex;
            while (Is('*'))
            {
                Eat('*');
            }

            SkipNonToken();

            if (IsWordStart() && precedence == int.MaxValue)
            {
                SetIndex(wordStart.StartIndex);
                LFCTypeToken type = ReadType(true);
                SkipNonToken();
                string name = ReadWord();
                SkipNonToken();
                LFCExpression[]? constructorArgs = null;
                if (Is('(') && TryReadInvocationArguments(result, codeSection, out (LFCTypeToken[], LFCExpression[]) output))
                {
                    if (output.Item1.Length != 0)
                    {
                        throw new LFCParserException("Cannot have type arguments on a constructor call", wordStart);
                    }

                    constructorArgs = output.Item2;
                }

                if (Is(';') || Is('='))
                {
                    return new LFCVariableDefinitionExpression(name, wordStart.Combine(CreatePosition()), type, constructorArgs);
                }

                SetIndex(prePointer);
                SkipNonToken();
            }
            else
            {
                SetIndex(prePointer);
            }

            string varName = word.Name;
            if (varName.EndsWith('*'))
            {
                SetIndex(wordStart.StartIndex + word.Name.Length);
                while (varName.EndsWith('*'))
                {
                    varName = varName.Remove(varName.Length - 1);
                }
            }

            if (Is(':') && Is(':', 1))
            {
                while (Is(':') && Is(':', 1))
                {
                    Eat(':');
                    Eat(':');
                    varName += $"::{ReadWord()}";
                }
            }

            return new LFCVariableExpression(varName, wordStart.Combine(CreatePosition()));
        }

        throw new LFCParserException("Invalid value", CreatePosition());
    }

    private LFCIfBranchExpression ParseIf(LFReaderResult result, LFReaderCodeSection codeSection, LFSourcePosition startPos)
    {
        Dictionary<LFCExpression, LFCExpression[]> branches = new Dictionary<LFCExpression, LFCExpression[]>();
        LFCExpression[]? elseBranch = null;
        (LFCExpression condition, LFCExpression[] body) = ParseIfPart(result, codeSection);
        branches.Add(condition, body);
        SkipNonToken();
        int start = CurrentIndex;
        Eat('}');
        SkipNonToken();
        string word = IsWordStart() ? ReadWord() : "";
        while (word == "else")
        {
            SkipNonToken();
            if (IsWordStart() && ReadWord() == "if")
            {
                (LFCExpression elseCondition, LFCExpression[] elseBody) = ParseIfPart(result, codeSection);
                branches.Add(elseCondition, elseBody);
                SkipNonToken();
                start = CurrentIndex;
                Eat('}');
                SkipNonToken();
                word = IsWordStart() ? ReadWord() : "";
            }
            else
            {
                SkipNonToken();
                Eat('{');
                SkipNonToken();
                elseBranch = ReadBlock(result, codeSection).ToArray();
                start = CurrentIndex;

                break;
            }
        }

        SetIndex(start);

        return new LFCIfBranchExpression(startPos, branches, elseBranch);
    }

    private (LFCExpression, LFCExpression[]) ParseIfPart(LFReaderResult result, LFReaderCodeSection codeSection)
    {
        SkipNonToken();
        Eat('(');
        SkipNonToken();
        LFCExpression condition = ReadExpression(result, codeSection);
        Eat(')');
        SkipNonToken();
        Eat('{');
        List<LFCExpression> exprs = ReadBlock(result, codeSection);

        return (condition, exprs.ToArray());
    }

    private bool TryReadInvocationArguments(LFReaderResult result, LFReaderCodeSection codeSection, out (LFCTypeToken[], LFCExpression[]) output)
    {
        try
        {
            LFCTypeToken[] typeArgs = Array.Empty<LFCTypeToken>();
            SkipNonToken();
            if (Is('<'))
            {
                Eat('<');
                SkipNonToken();
                List<LFCTypeToken> args = new List<LFCTypeToken>();
                args.Add(ReadType(false));
                SkipNonToken();
                while (Is(','))
                {
                    Eat(',');
                    SkipNonToken();
                    args.Add(ReadType(false));
                    SkipNonToken();
                }

                SkipNonToken();
                Eat('>');
                typeArgs = args.ToArray();
            }

            SkipNonToken();
            Eat('(');
            SkipNonToken();
            List<LFCExpression> parameters = new List<LFCExpression>();
            if (!Is(')'))
            {
                parameters.Add(ReadExpression(result, codeSection));
                SkipNonToken();
                while (Is(','))
                {
                    Eat(',');

                    SkipNonToken();
                    parameters.Add(ReadExpression(result, codeSection));
                }
            }

            Eat(')');

            output = (typeArgs, parameters.ToArray());

            return true;
        }
        catch (Exception)
        {
            //Logger.Error(ex);
            output = (null, null)!;

            return false;
        }
    }

    private bool TryParseInvocation(out LFCExpression? output, LFReaderCodeSection codeSection, LFCExpression left, LFReaderResult result)
    {
        LFCPreProcessor pp = LFCPreProcessor.GetPreProcessor(result);
        if (!TryReadInvocationArguments(result, codeSection, out (LFCTypeToken[], LFCExpression[]) arguments))
        {
            output = null;

            return false;
        }

        if (left is LFCVariableExpression vExpr && pp.HasMacro(vExpr.Name))
        {
            if (arguments.Item1.Length != 0)
            {
                throw new LFCParserException("Invalid number of type arguments", vExpr.Position);
            }

            LFCPreProcessorMacro macro = pp.GetMacro(vExpr.Name);
            if (macro.IsBatchMacro)
            {
                List<LFCExpression> exprs = new List<LFCExpression>();
                foreach (LFCExpression arg in arguments.Item2)
                {
                    exprs.Add(macro.PrepareExpression(new[] { arg }));
                }

                output = new LFCBlockExpression(exprs.ToArray());
            }
            else
            {
                output = macro.PrepareExpression(arguments.Item2);
            }

            return true;
        }

        output = new LFCInvocationExpression(left.Position.Combine(CreatePosition()), left, arguments.Item2, arguments.Item1);

        return true;
    }

    public LFCExpression ReadExpression(
        LFReaderResult result,
        LFReaderCodeSection currentSection,
        LFCExpression? left = null,
        int precedence = int.MaxValue)
    {
        SkipNonToken();
        left ??= ReadValue(result, currentSection, precedence);

        while (!IsEOF())
        {
            SkipNonToken();

            if (left == null)
            {
                throw new LFCParserException("Left is null", CreatePosition());
            }

            if (Is(';') || Is('}') || Is(',') || Is(')') || Is(']') || Is(':'))
            {
                return left;
            }

            if (Is('['))
            {
                Eat('[');
                LFCExpression right = ReadExpression(result, currentSection);
                SkipNonToken();
                Eat(']');

                left = new LFCArrayAccessExpression(left.Position.Combine(CreatePosition()), left, right);

                continue;
            }

            if (Is('(') || Is('<'))
            {
                int start = CurrentIndex;
                if (!TryParseInvocation(out LFCExpression? lVal, currentSection, left, result))
                {
                    SetIndex(start);
                }
                else
                {
                    left = lVal;

                    continue;
                }
            }

            LFSourcePosition symPos = CreatePosition();
            string sym = ReadUntilWordOrWhiteSpace();
            symPos = symPos.Combine(CreatePosition());

            if (sym.StartsWith(')'))
            {
                MovePrevious(sym.Length);

                return left;
            }

            if (sym == "")
            {
                throw new LFCParserException("Invalid expression", CreatePosition());
            }

            LFCPostValueOperator op = LFCOperatorTable.GetPostValueOperator(sym, symPos);

            if (op.Precedence > precedence)
            {
                MovePrevious(sym.Length);

                break;
            }

            left = op.Parse(new LFCPostValueOperatorParseInput(left, currentSection, this, result));
        }

        if (left == null)
        {
            throw new LFCParserException("Left is null", CreatePosition());
        }

        return left;
    }

    public List<LFCExpression> ReadBlock(LFReaderResult result, LFReaderCodeSection codeSection)
    {
        List<LFCExpression> expressions = new List<LFCExpression>();
        while (!Is('}'))
        {
            LFCExpression expr = ReadExpression(result, codeSection);
            SkipNonToken();
            while (!Is(';') && !Is('}'))
            {
                expr = ReadExpression(result, codeSection, expr);
            }

            expressions.Add(expr);
            if (Is('}'))
            {
                Eat('}');
            }
            else
            {
                Eat(';');
            }

            SkipNonToken();
        }

        return expressions;
    }


    private void ReadContent(LFReaderResult result, LFReaderCodeSection codeSection)
    {
        SkipNonToken();

        if (Is('#'))
        {
            LFSourcePosition cmdPos = CreatePosition();
            Eat('#');
            string cmd = ReadWord();
            cmdPos = cmdPos.Combine(CreatePosition());
            SkipNonToken();
            if (cmd == "link")
            {
                string path;
                if (Is('<'))
                {
                    Eat('<');
                    SkipNonToken();
                    path = ReadWord();
                    SkipNonToken();
                    Eat('>');
                    SkipNonToken();
                    path = CoreLibraryHelper.GetCoreLibrary(path);
                }
                else
                {
                    path = ReadString();
                }

                result.AddFileRequire(ProcessDirectiveValue(path, result));
            }
            else if (cmd == "entry")
            {
                string entryPoint = ProcessDirectiveValue(ReadWord(), result);
                EmitEntryCode(result, entryPoint);
                codeSection.AddExport(entryPoint);
            }
            else if (cmd == "export")
            {
                codeSection.AddExport(ProcessDirectiveValue(ReadWord(), result));
            }
            else if (cmd == "import")
            {
                codeSection.AddImport(ProcessDirectiveValue(ReadWord(), result));
            }
            else if (cmd == "define")
            {
                LFCPreProcessor pp = LFCPreProcessor.GetPreProcessor(result);
                string defName = ProcessDirectiveValue(ReadWord(), result);
                SkipNonToken();
                string defValue = ProcessDirectiveValue(ReadElement(), result);
                pp.Define(defName, defValue);
            }
            else if (cmd == "undefine")
            {
                LFCPreProcessor pp = LFCPreProcessor.GetPreProcessor(result);
                string defName = ProcessDirectiveValue(ReadWord(), result);
                pp.UnDefine(defName);
            }
            else if (cmd == "include")
            {
                string includeFile = ProcessDirectiveValue(ReadString(), result);
                result.AddInclude(Path.Combine(WorkingDirectory, includeFile));
            }
            else if (cmd == "inline")
            {
                string incFileStr = ProcessDirectiveValue(ReadString(), result);
                string includeFile = Path.Combine(WorkingDirectory, incFileStr);
                string source = File.ReadAllText(includeFile);
                LFCReader r = new LFCReader(source, WorkingDirectory, includeFile, codeSection);
                r.ReadToEnd(result);
            }
            else if (cmd == "macro")
            {
                string macroName = ProcessDirectiveValue(ReadWord(), result);
                SkipNonToken();
                string[] signature = ParseMacroSignature();
                SkipNonToken();
                LFCExpression expr = ReadExpression(result, codeSection);
                SkipNonToken();
                Eat(';');
                SkipNonToken();

                LFCPreProcessor pp = LFCPreProcessor.GetPreProcessor(result);
                LFCPreProcessorMacro macro = new LFCPreProcessorMacro(signature, expr);
                pp.AddMacro(macroName, macro);
            }
            else if (cmd == "batch_macro")
            {
                string macroName = ProcessDirectiveValue(ReadWord(), result);
                SkipNonToken();
                string[] signature = ParseMacroSignature();

                if (signature.Length != 1)
                {
                    throw new LFCParserException("Batch macro must have one argument", cmdPos);
                }

                SkipNonToken();
                LFCExpression expr = ReadExpression(result, codeSection);
                SkipNonToken();
                Eat(';');
                SkipNonToken();

                LFCPreProcessor pp = LFCPreProcessor.GetPreProcessor(result);
                LFCPreProcessorMacro macro = new LFCPreProcessorMacro(signature, expr, true);
                pp.AddMacro(macroName, macro);
            }
            else if (cmd == "ifdef" || cmd == "ifndef")
            {
                SkipNonToken();
                string macroName = ProcessDirectiveValue(ReadWord(), result);
                SkipNonToken();
                LFCPreProcessor pp = LFCPreProcessor.GetPreProcessor(result);
                Eat('{');
                bool invert = cmd == "ifndef";
                if (!pp.IsDefined(macroName) && !invert ||
                    pp.IsDefined(macroName) && invert)
                {
                    int openBraces = 1;
                    while (openBraces > 0)
                    {
                        SkipNonToken();
                        if (Is('"'))
                        {
                            ReadString();
                        }

                        SkipNonToken();
                        if (Is('{'))
                        {
                            openBraces++;
                        }
                        else if (Is('}'))
                        {
                            openBraces--;
                        }

                        MoveNext();
                    }
                }
                else
                {
                    int openBraces = 1;
                    int start = CurrentIndex;
                    int end = -1;
                    while (openBraces > 0)
                    {
                        SkipNonToken();
                        if (Is('"'))
                        {
                            ReadString();
                        }

                        SkipNonToken();
                        if (Is('{'))
                        {
                            openBraces++;
                        }
                        else if (Is('}'))
                        {
                            openBraces--;
                            end = CurrentIndex;
                        }

                        MoveNext();
                    }

                    ReplaceCharacter(end, ' ');
                    SetIndex(start);
                }

                SkipNonToken();
            }
            else if (cmd == "if" || cmd == "ifn")
            {
                SkipNonToken();
                string macroName = ProcessDirectiveValue(ReadWord(), result);
                SkipNonToken();
                string macroValue = ProcessDirectiveValue(ReadElement(), result);
                SkipNonToken();

                LFCPreProcessor pp = LFCPreProcessor.GetPreProcessor(result);
                if (!pp.IsDefined(macroName))
                {
                    throw new LFCParserException("Macro not defined", cmdPos);
                }

                Eat('{');
                bool invert = cmd == "ifn";
                if (pp.GetDefine(macroName) != macroValue && !invert ||
                    pp.GetDefine(macroName) == macroValue && invert)
                {
                    int openBraces = 1;
                    while (openBraces > 0)
                    {
                        SkipNonToken();
                        if (Is('"'))
                        {
                            ReadString();
                        }

                        SkipNonToken();
                        if (Is('{'))
                        {
                            openBraces++;
                        }
                        else if (Is('}'))
                        {
                            openBraces--;
                        }

                        MoveNext();
                    }
                }
                else
                {
                    int openBraces = 1;
                    int start = CurrentIndex;
                    int end = -1;
                    while (openBraces > 0)
                    {
                        SkipNonToken();
                        if (Is('"'))
                        {
                            ReadString();
                        }

                        SkipNonToken();
                        if (Is('{'))
                        {
                            openBraces++;
                        }
                        else if (Is('}'))
                        {
                            openBraces--;
                            end = CurrentIndex;
                        }

                        MoveNext();
                    }

                    ReplaceCharacter(end, ' ');
                    SetIndex(start);
                }

                SkipNonToken();
            }
            else if (cmd == "warning")
            {
                string msg = ProcessDirectiveValue(ReadString(), result);
                Logger.Warning($"[WARNING] {msg}");
            }
            else if (cmd == "error")
            {
                string msg = ProcessDirectiveValue(ReadString(), result);
                Logger.Error($"[ERROR] {msg}");

                throw new LFCParserException($"Preprocess Error: {msg}", cmdPos);
            }
            else
            {
                throw new LFCParserException($"Unknown command {cmd}", cmdPos);
            }

            SkipNonToken();

            return;
        }


        LFSourcePosition typeStart = CreatePosition();
        LFCTypeToken type = ReadType(true);


        if (type.Name == "struct")
        {
            SkipNonToken();
            LFSourcePosition typePos = CreatePosition();
            LFCTypeToken typeName = ReadType(false);
            typePos = typePos.Combine(CreatePosition());
            SkipNonToken();
            Eat('{');
            SkipNonToken();
            Dictionary<string, LFCTypeToken> members = new Dictionary<string, LFCTypeToken>();
            List<LFCFunction> operatorOverrides = new List<LFCFunction>();
            List<LFCFunction> functions = new List<LFCFunction>();
            while (!Is('}'))
            {
                LFSourcePosition position = CreatePosition();
                LFCTypeToken memberType = ReadType(true);
                bool isOverride = false;

                SkipNonToken();

                if (Is('(') && memberType.Name == typeName.Name)
                {
                    LFCFunction function = ParseFunction(result, codeSection, new LFCTypeToken("void", Array.Empty<LFCTypeToken>()), ".ctor", false, position);

                    //Add Hidden first parameter
                    LFCFunction thisFunc = function.CreateInstance(typeName);
                    thisFunc.AutoExport = true;
                    result.OnFinalize(() => thisFunc.Compile(result));

                    functions.Add(thisFunc);

                    continue;
                }

                if (memberType.Name == "override")
                {
                    memberType = ReadType(true);
                    isOverride = true;
                    SkipNonToken();
                }

                string memberName = ReadWord();
                SkipNonToken();
                if (Is(';'))
                {
                    if (isOverride)
                    {
                        throw new LFCParserException("Override not supported on fields", position);
                    }

                    //Field
                    Eat(';');
                    members.Add(memberName, memberType);
                }
                else if (Is('<') || Is('('))
                {
                    LFCFunction function = ParseFunction(result, codeSection, memberType, memberName, false, position);

                    if (!isOverride)
                    {
                        //Add Hidden first parameter
                        LFCFunction thisFunc = function.CreateInstance(typeName);
                        result.OnFinalize(() => thisFunc.Compile(result));
                        thisFunc.AutoExport = true;
                        functions.Add(thisFunc);
                    }
                    else
                    {
                        LFCFunction opFunc = function.CreateOperator(typeName);
                        result.OnFinalize(() => opFunc.Compile(result));

                        opFunc.AutoExport = true;
                        operatorOverrides.Add(opFunc);

                        LFCFunctionTable.GetFunctionTable(codeSection, result).AddFunction(opFunc);
                    }
                }
                else
                {
                    throw new LFCParserException("Unknown token", CreatePosition());
                }

                SkipNonToken();
            }

            Eat('}');

            LFCReaderStructureType structType = new LFCReaderStructureType(typePos, typeName, members, functions, codeSection, operatorOverrides);
            LFCTypeDatabase db = LFCTypeDatabase.GetTypeSystem(result);
            if (typeName.TypeArgs.Length != 0)
            {
                db.AddType(structType);
            }
            else
            {
                LFCType structT = structType.CreateType(db, new Dictionary<LFCTypeToken, LFCTypeToken>());
            }

            return;
        }

        SkipNonToken();

        ParseFunction(result, codeSection, type, typeStart);
    }

    private LFCFunction ParseFunction(LFReaderResult result, LFReaderCodeSection codeSection, LFCTypeToken type, string name, bool isExtern, LFSourcePosition position)
    {
        SkipNonToken();

        LFCTypeToken[] typeArgs = Array.Empty<LFCTypeToken>();
        if (Is('<'))
        {
            if (isExtern)
            {
                throw new LFCParserException("Extern functions cannot have generic parameters", CreatePosition());
            }

            Eat('<');
            SkipNonToken();
            List<LFCTypeToken> genericParameters = new List<LFCTypeToken>();
            genericParameters.Add(ReadType(false));
            SkipNonToken();
            while (Is(','))
            {
                Eat(',');
                SkipNonToken();
                genericParameters.Add(ReadType(false));
                SkipNonToken();
            }

            SkipNonToken();
            Eat('>');
            SkipNonToken();
            typeArgs = genericParameters.ToArray();
        }

        SkipNonToken();
        Eat('(');
        SkipNonToken();
        bool hasNext = !IsEOF() && !Is(')');

        List<LFCFunctionParameter> parameters = new List<LFCFunctionParameter>();

        while (hasNext)
        {
            SkipNonToken();
            LFCTypeToken paramType = ReadType(true);
            SkipNonToken();
            string paramName = ReadWord();
            SkipNonToken();
            hasNext = Is(',');
            if (hasNext)
            {
                Eat(',');
            }

            parameters.Add(new LFCFunctionParameter(paramName, paramType));
        }

        Eat(')');

        position = position.Combine(CreatePosition());
        SkipNonToken();
        List<LFCExpression> body = new List<LFCExpression>();
        if (isExtern)
        {
            Eat(';');
        }
        else
        {
            Eat('{');
            SkipNonToken();
            body = ReadBlock(result, codeSection);
            SkipNonToken();
            Eat('}');
        }

        SkipNonToken();
        LFCFunction func;
        if (isExtern)
        {
            LFAssemblySymbol symbol = name;
            func = new LFCExternFunction(name, type, parameters.ToArray(), codeSection, position);
            codeSection.AddImport(symbol.Parts[1]);
        }
        else
        {
            func = new LFCBlockFunction(name, type, parameters.ToArray(), typeArgs, body.ToArray(), codeSection, position);
        }

        LFCFunctionTable.GetFunctionTable(codeSection, result).AddFunction(func);


        return func;
    }

    private LFCFunction ParseFunction(LFReaderResult result, LFReaderCodeSection codeSection, LFCTypeToken type, LFSourcePosition position)
    {
        bool isExtern = false;
        if (type.Name == "extern")
        {
            SkipNonToken();
            type = ReadType(true);
            SkipNonToken();
            isExtern = true;
        }

        string name = ReadWord();
        if (isExtern)
        {
            while (Is(':') && Is(':', 1))
            {
                Eat(':');
                Eat(':');
                name += $"::{ReadWord()}";
            }
        }

        LFCFunction func = ParseFunction(result, codeSection, type, name, isExtern, position);
        result.OnFinalize(() => func.Compile(result));

        return func;
    }

    protected override void Read(LFReaderResult result)
    {
        LFReaderCodeSection codeSection = m_DefaultSection ?? result.GetOrCreateCodeSection(result.Name);
        result.GetOrCreateDataSection($"{result.Name}_data");
        codeSection.AddImport($"{result.Name}_data");

        SkipNonToken();
        if (IsWordStart())
        {
            int start = CurrentIndex;
            string word = ReadWord();
            if (word == "namespace")
            {
                SkipNonToken();
                string nsName = ReadWord();
                codeSection = result.GetOrCreateCodeSection(nsName);
                SkipNonToken();
                Eat('{');
                SkipNonToken();
                while (!Is('}'))
                {
                    ReadContent(result, codeSection);
                    SkipNonToken();
                }

                Eat('}');
                SkipNonToken();

                return;
            }

            SetIndex(start);
        }

        ReadContent(result, codeSection);
        SkipNonToken();
    }

    private string[] ParseMacroSignature()
    {
        List<string> str = new List<string>();

        SkipNonToken();
        Eat('(');
        SkipNonToken();
        while (!Is(')'))
        {
            SkipNonToken();
            str.Add(ReadWord());
            SkipNonToken();
            if (Is(')'))
            {
                break;
            }

            SkipNonToken();
            Eat(',');
        }

        Eat(')');
        SkipNonToken();

        return str.ToArray();
    }

    private string ReadElement()
    {
        StringBuilder sb = new StringBuilder();
        while (!IsEOF() && !IsWhiteSpace())
        {
            sb.Append(CurrentChar);
            MoveNext();
        }

        return sb.ToString();
    }


    protected string ProcessDirectiveValue(string value, LFReaderResult result)
    {
        LFCPreProcessor pp = LFCPreProcessor.GetPreProcessor(result);
        bool replaced = true;
        while (replaced)
        {
            replaced = false;
            foreach (string define in pp.Defines)
            {
                string defStr = $"$({define})";
                if (value.Contains(defStr))
                {
                    value = value.Replace(defStr, pp.GetDefine(define));
                    replaced = true;
                }
            }
        }

        return value;
    }
}