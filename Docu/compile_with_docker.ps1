$currentDir = Get-Location
$image = "texlive/texlive:latest"

# Check if docker is running
docker info 2>&1 | Out-Null
if ($LASTEXITCODE -ne 0) {
    Write-Host "Docker is not running. Please start Docker Desktop." -ForegroundColor Red
    exit 1
}

Write-Host "Pulling/Using LaTeX Docker image ($image)..."
docker pull $image

Write-Host "Compiling main.tex..."
# Mount current directory to /workdir in container
# Run latexmk
docker run --rm -v "${currentDir}:/workdir" -w /workdir $image latexmk -pdf -interaction=nonstopmode -synctex=1 -g main.tex

if ($?) {
    Write-Host "Compilation successful!" -ForegroundColor Green
} else {
    Write-Host "Compilation failed." -ForegroundColor Red
}
