#!/bin/bash

DEST="/usr/local/bin/wdrop"

WDROP_DIR="./out/wdrop"

OS=$(uname)
RID=$(dotnet --info | grep -i rid | awk '{print $2}')

if [ ! -f $WDROP_DIR ]; then
    echo "❌ Arquivo 'wdrop' não encontrado no diretório atual."
    echo "Compile com: dotnet publish -c Release -r $RID --self-contained true -p:PublishSingleFile=true -o ./out"
    exit 1
fi

echo "🚀 Instalando wdrop em $DEST..."
sudo cp $WDROP_DIR $DEST
sudo chmod +x $DEST

echo "✅ windrop instalado com sucesso!"
echo "Use: wdrop <file>"
