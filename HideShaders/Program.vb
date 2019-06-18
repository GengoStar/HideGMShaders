Imports System.IO
Imports System.Text
Imports System.Text.Encoding

Module Program
    ReadOnly FillBytes As Byte() = ASCII.GetBytes("AH")
    Sub Donʼt(text As String)
        Throw New Exception(text)
    End Sub
    Function GetString(File As FileStream, WR As BinaryReader, Pos As Int32) As String
        Dim Old = File.Position
        File.Position = Pos
        Dim SB = New StringBuilder()
        Dim Ch = WR.ReadChar()
        While Ch <> Chr(0)
            SB.Append(Ch)
            Ch = WR.ReadChar()
        End While
        File.Position = Old
        Return SB.ToString()
    End Function
    Sub EraseString(File As FileStream, Pos As Int32)
        Dim Old = File.Position
        File.Position = Pos
        Dim Length = 0
        While File.Position < File.Length
            Dim Ch = File.ReadByte()
            If Ch = 0 Then Exit While
            Length += 1
        End While
        File.Position = Pos
        For ChInd = 0 To Length - 1
            File.WriteByte(FillBytes(ChInd Mod FillBytes.Length))
        Next
        File.Position = Old
    End Sub
    Sub DoAll(path As String)
        Dim Win = File.Open(path, FileMode.Open, FileAccess.ReadWrite)
        Dim WR = New BinaryReader(Win)
        If (WR.ReadChars(4) <> "FORM") Then Donʼt("Illegal File")
        Dim Left = WR.ReadInt32()
        While Left > 0
            Dim ChName = WR.ReadChars(4)
            Dim ChSize = WR.ReadInt32()
            Dim ChEnd = Win.Position + ChSize
            'Console.WriteLine(ChName & " " & ChSize)
            If ChName = "SHDR" Then
                Dim ShCount = WR.ReadInt32()
                If ShCount = 0 Then Donʼt("No shaders here")
                Console.WriteLine(ShCount & " shaders")
                Dim ShAts(ShCount) As Int32
                For ShInd = 0 To ShCount
                    ShAts(ShInd) = WR.ReadInt32()
                Next
                For ShInd = 0 To ShCount - 1
                    Win.Position = ShAts(ShInd)
                    Dim AtName = WR.ReadInt32()
                    Console.WriteLine("Hiding " & GetString(Win, WR, AtName))
                    Dim ShType = WR.ReadInt32()
                    EraseString(Win, WR.ReadInt32()) ' GLSLESV
                    EraseString(Win, WR.ReadInt32()) ' GLSLESF
                    EraseString(Win, WR.ReadInt32()) ' GLSLV
                    EraseString(Win, WR.ReadInt32()) ' GLSLF
                    EraseString(Win, WR.ReadInt32()) ' HLSL9V
                    EraseString(Win, WR.ReadInt32()) ' HLSL9F
                Next
                Return
            End If
            Win.Position = ChEnd
            Left -= ChSize + 8
        End While
        Donʼt("No shader chunk")
    End Sub
    Sub Main(args As String())
        If args.Length = 0 Then
            Console.WriteLine("Drag your win file here")
        Else
            Try
                DoAll(args(0))
                Console.WriteLine("We did it")
            Catch ex As Exception
                Console.WriteLine(ex)
            End Try
        End If
        Console.WriteLine("Any key to exit")
        Console.ReadKey()
    End Sub
End Module
