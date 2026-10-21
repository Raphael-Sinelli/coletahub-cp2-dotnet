<#
.SYNOPSIS
    Gera o ColetaHub_CP2.zip de entrega do Checkpoint 2.

.DESCRIPTION
    1. Compila a solution em Debug e em Release (cria src/ColetaHub.Web/bin/Debug e bin/Release).
    2. Roda os testes xUnit.
    3. Compacta a pasta inteira do projeto em ColetaHub_CP2.zip, na raiz, com a pasta ColetaHub_CP2 dentro
       do zip. Ficam de fora os arquivos ignorados pelo git (.vs, node_modules, o próprio zip), exceto
       bin e obj, que fazem parte da entrega.

    Feche o Visual Studio e pare a aplicação antes de executar (arquivos em uso impedem o build).

.EXAMPLE
    ./empacotar.ps1
#>
$ErrorActionPreference = 'Stop'

$raiz = $PSScriptRoot
$solution = Join-Path $raiz 'ColetaHub.sln'
$nomeZip = 'ColetaHub_CP2.zip'
$arquivoZip = Join-Path $raiz $nomeZip

function Invoke-Etapa([string] $descricao, [scriptblock] $comando) {
    Write-Host "==> $descricao" -ForegroundColor Cyan
    & $comando
    if ($LASTEXITCODE -ne 0) {
        throw "Falhou: $descricao (código $LASTEXITCODE)."
    }
}

Invoke-Etapa 'Restaurando pacotes' { dotnet restore $solution }
Invoke-Etapa 'Build Debug' { dotnet build $solution -c Debug --no-restore }
Invoke-Etapa 'Build Release' { dotnet build $solution -c Release --no-restore }
Invoke-Etapa 'Testes xUnit' { dotnet test $solution -c Debug --no-build }

foreach ($configuracao in 'Debug', 'Release') {
    $pastaBin = Join-Path $raiz "src\ColetaHub.Web\bin\$configuracao"
    if (-not (Test-Path $pastaBin)) {
        throw "Pasta não encontrada: $pastaBin"
    }
}

Write-Host "==> Gerando $nomeZip (sem arquivos ignorados pelo git, exceto bin e obj)" -ForegroundColor Cyan
if (Test-Path $arquivoZip) {
    Remove-Item $arquivoZip -Force
}

# Tudo o que o .gitignore exclui (.vs, node_modules, o próprio zip...) fica fora, menos bin e obj,
# que a entrega exige. Pastas aparecem com "/" no final e excluem todo o conteúdo.
$ignorados = @(git -C $raiz -c core.quotepath=off ls-files --others --ignored --exclude-standard --directory |
    Where-Object { $_ -notmatch '(^|/)(bin|obj)/' })
if ($LASTEXITCODE -ne 0) {
    throw 'Falhou: listar arquivos ignorados pelo git.'
}

$arquivos = Get-ChildItem $raiz -Recurse -File -Force | Where-Object {
    $relativo = $_.FullName.Substring($raiz.Length + 1).Replace('\', '/')
    -not ($ignorados | Where-Object { $relativo -eq $_ -or ($_.EndsWith('/') -and $relativo.StartsWith($_)) })
}

Add-Type -AssemblyName System.IO.Compression, System.IO.Compression.FileSystem
$zip = [IO.Compression.ZipFile]::Open($arquivoZip, [IO.Compression.ZipArchiveMode]::Create)
try {
    foreach ($arquivo in $arquivos) {
        # Caminhos com "/" para o zip abrir corretamente também fora do Windows.
        $entrada = 'ColetaHub_CP2/' + $arquivo.FullName.Substring($raiz.Length + 1).Replace('\', '/')
        [IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip, $arquivo.FullName, $entrada, [IO.Compression.CompressionLevel]::Optimal) | Out-Null
    }
}
finally {
    $zip.Dispose()
}

$tamanhoMb = [math]::Round((Get-Item $arquivoZip).Length / 1MB, 1)
Write-Host "Pronto: $arquivoZip ($($arquivos.Count) arquivos, $tamanhoMb MB)" -ForegroundColor Green
