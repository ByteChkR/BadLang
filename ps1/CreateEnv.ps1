$bs = "$pwd"

$bs = (Get-Item $bs).fullname

$Env:Path += ";$bs"
echo "Added '$pwd' to environment path for this session"