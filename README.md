# PWAAT Extractor Suite
PWAAT Extractor Suite is an open-source tool designed for extracting and inserting assets in Phoenix Wright: Ace Attorney Trilogy.

## Installation
1. Download the precompiled executable from [Releases](https://github.com/MaFIaTH/PWAATExtractorSuite/releases).
The executable comes in two versions: with and without .NET.
   - If you choose the version with .NET, you do not need to download .NET Runtime 9.0.
   - If you choose the version without .NET, you will need to download [.NET Runtime 9.0](https://dotnet.microsoft.com/en-us/download/dotnet/9.0).
3. Extract the ZIP file.
4. Run the application.

Alternatively, you can pull the repository and build with .NET SDK 9.0.

# Features
## All-in-one
This suite brings together all the tools you need to modify the game! Including:
### Binary Extractor (.bin or .cho)
Extract the .bin or .cho files into .json format for ease of editing:
```JSON
{
  "FileName": "credit_text.bin",
  "TextList": [
    {
      "Index": 0,
      "Original": "0<Break>Multi-Platform Port Staff\r",
      "Translation": "0<Break>Multi-Platform Port Staff\r"
    },
    {
      "Index": 1,
      "Original": "1<Break>Digital Works Entertainment\r",
      "Translation": "1<Break>Digital Works Entertainment\r"
    }
  ]
}
```
### Scenario Extractor and Simplifier (.mdt) 
Extract all text and commands from the .mdt files into .txt:
```
SetSpeakerId(512);
SetTextColor(2);
Text("(เด็กผู้หญิงที่นั่งอยู่ตรงหน้า");
NewLine();
Text("ฉันเนี่ยนะ เป็นฆาตกร!?)");
ReadKey();
Text("(ถึงหลักฐานทั้งหมด");
NewLine();
Text("จะชี้ว่า "เธอ" เป็นคนทำ)");
ReadKey();
Text("(แต่การที่มันเป็นแบบนั้น");
NewLine();
Text("ยิ่งทำให้มันน่าสงสัย");
NewLine();
Text("เข้าไปอีก)");
SetTextColor(0);
ReadKey();
Op_1C(1);
PlayFadeCtrl(513, 1, 31);
Wait(7);
PlayCharacterAnimation(22, 173, 173);
SetBackground(32);
PlayFadeCtrl(257, 1, 31);
Wait(7);
Op_1C(0);
SetTextColor(2);
Text("(และด้วยท่าทีของพยาน");
NewLine();
Text("ที่ดูแปลกๆ... ");
Wait(15);
Text("ไม่น่าจะใช่");
NewLine();
Text("เรื่องปกติอย่างแน่นอน)");
SetTextColor(0);
ReadKey();
```
The Simplifier can simplify the text, including only the commands necessary for translation:
```
[Phoenix Wright]
[SetTextColor(Blue);]
#"(เด็กผู้หญิงที่นั่งอยู่ตรงหน้า"
[NewLine();]
#"ฉันเนี่ยนะ เป็นฆาตกร!?)"
[ReadKey();]
#"(ถึงหลักฐานทั้งหมด"
[NewLine();]
#"จะชี้ว่า "เธอ" เป็นคนทำ)"
[ReadKey();]
#"(แต่การที่มันเป็นแบบนั้น"
[NewLine();]
#"ยิ่งทำให้มันน่าสงสัย"
[NewLine();]
#"เข้าไปอีก)"
[SetTextColor(White);]
[ReadKey();]
[SetTextColor(Blue);]
#"(และด้วยท่าทีของพยาน"
[NewLine();]
#"ที่ดูแปลกๆ... "
[Wait(15);]
#"ไม่น่าจะใช่"
[NewLine();]
#"เรื่องปกติอย่างแน่นอน)"
[SetTextColor(White);]
[ReadKey();]
```
### Decrypt/encrypt (any file)
The game files are encrypted by default. The decryptor can decrypt the files so that they can be read and written by the Asset Editor and other modding tools.

## Workspace
The Workspace feature allows you to save the working directory as a .pwaatws file.

You no longer need to select folders every time you want to extract the game files.

The Workspace Wizard also helps you set up the working directory and the necessary files. 

All you have to do is put the file in the working folder and click 'Start operation'. It's that simple!

## Credits
The original extraction tools were developed by the Ace Attorney Vietnam team.
As I could not locate the source code, I decompiled the tools and rebuilt them for this application.
If you know where I can find the source code, please contact me and I will make sure to give proper credits.
