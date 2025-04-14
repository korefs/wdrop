#!/bin/bash

DEST="/usr/local/bin/wdrop"

if [ ! -f "wdrop" ]; then
    echo "❌ Arquivo 'wdrop' não encontrado no diretório atual."
    echo "Compile com: dotnet publish -c Release -r osx-x64 --self-contained true -p:PublishSingleFile=true -o ./out"
    exit 1
fi

echo "🚀 Instalando wdrop em $DEST..."
sudo cp wdrop $DEST
sudo chmod +x $DEST

echo "✅ windrop instalado com sucesso!"
echo "Use: wdrop <file>"
