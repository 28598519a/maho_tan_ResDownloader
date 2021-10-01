# maho_tan_ResDownloader
用來抓 妖女奇譚 X指定 的遊戲資源

此遊戲基於Cocos2dxJS引擎，因此要抓下所有資源文件必須慢慢翻Code去找出不同資源文件Url的產生機制，並不容易
由於是直接模擬產生出Url的機制，因此甚至有可能抓到正常遊玩不會載到的資源，但也有可能會漏掉少數非常不重要的資源類別

建議先抓遊戲Android版的[APK](https://www.johren.games/games/app/maho-tan-zh-tw/)安裝於root設備上，完成新手教學後到處點一點在退出
之後至data/data/game.johren.mahorobat中複製出hotupdate、https_、local_res這三個資料夾及裡面的所有東西放至同一個資料夾下

執行maho_tan資源下載器，按下載選擇剛剛用來放那三個資料夾的資料夾，此時應開始下載遊戲資源，全部載完後總遊戲資源大小應接近3.7GB (繁中版)

(如果不想安裝APK，可以直接解壓Release內附的ExampleFolder.zip直接用這個來載，但可能會有少數一進遊戲就能全部取得的資源類別不會被下載到)
