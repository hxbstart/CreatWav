Imports System.IO
Imports System.Math


Public Class Form1
    Structure waveHeaderStructre
        'RiffChunk
        Dim RIFF() As Char
        Dim FileSize As UInteger
        Dim WAVE() As Char
        'FormatChunk
        Dim FORMAT() As Char
        Dim FormatSize As UInteger
        Dim FilePadding As UShort
        Dim FormatChannels As UShort
        Dim SamplesPerSecond As UInteger
        Dim AverageBytesPerSecond As UInteger
        Dim BytesPerSample As UShort
        Dim BitsPerSample As UShort
        'Dim FormatExtra As UShort
        'FactChunk
        ' Dim FACT As String
        ' Dim FactSize As UInteger
        ' Dim FactInf As UInteger
        'DataChunk
        Dim DATA() As Char
        Dim DataSize As UInteger
    End Structure
    Dim bshowFilename As String
    ' Private sub change2normal
    Private Sub showWave(picbox As PictureBox, ByVal data() As Byte)
        Dim g As Graphics = picbox.CreateGraphics
        g.Clear(SystemColors.ControlLight)
        Dim axialPen As Pen = New Pen(Color.Blue, 1)
        Dim picXdot As Integer
        Dim picYdot As Integer
        picYdot = Int(picbox.Height / 2)
        picXdot = 5
        '变换为数学坐标系
        g.TranslateTransform(picXdot, picYdot)
        g.ScaleTransform(1, -1)
        Dim startX As Integer = -5
        Dim endX As Integer = picbox.Width - 10
        Dim startY As Integer = 0
        Dim endY As Integer = 0
        'draw x axial
        g.DrawLine(axialPen, startX, startY, endX, endY)
        'draw 箭头
        g.DrawLine(axialPen, endX - 5, 5, endX, endY)
        g.DrawLine(axialPen, endX - 5, -5, endX, endY)
        'drawing Y axial
        g.DrawLine(axialPen, 0, -picYdot, 0, picYdot)
        g.DrawLine(axialPen, -5, picYdot - 5, 0, picYdot)
        g.DrawLine(axialPen, 5, picYdot - 5, 0, picYdot)
        Dim mBrush As New SolidBrush(Color.Blue)
        Dim mFont As New Font("宋体", 10)
        g.DrawString("x", mFont, mBrush, endX - 10, 5)
        For x = 0 To endX - 10 Step 10
            g.DrawLine(axialPen, x, 0, x, 2)
        Next
        For y = -picYdot + 10 To picYdot - 10 Step 10
            g.DrawLine(axialPen, 0, y, 2, y)
        Next
        'up code drawing axial finshed
        ' drawing wave start
        Dim bpen As Pen = New Pen(Color.Red, 1)
        Dim y1 As Integer
        Dim y2 As Integer
        Dim x1 As Integer
        Dim x2 As Integer
        Dim xdiv As Double
        xdiv = TextBox7.Text
        Dim ydiv As Double
        ydiv = TextBox8.Text
        For x = 0 To data.Length - 2
            x1 = Int(x / xdiv)
            y1 = Int(data(x) / ydiv) - picYdot / 2
            x2 = Int((x + 1) / xdiv)
            y2 = Int(data(x + 1) / ydiv) - picYdot / 2
            g.DrawLine(bpen, x1, y1, x2, y2)
        Next
        g.Dispose()
    End Sub
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim sampleTime, sampleHz, channel, channelBit, happenHz As Integer
        sampleTime = TextBox1.Text
        sampleHz = TextBox3.Text
        channel = TextBox2.Text
        channelBit = TextBox4.Text
        happenHz = TextBox6.Text
        If (Int(sampleHz / happenHz) < 2) Then
            MsgBox("采样频率必须是生成频率的2倍以上！")
            Return
        End If
        Dim fileSizeTotal As UInteger
        Dim PCMDateSize As UInteger
        PCMDateSize = sampleTime * sampleHz * channel
        Dim sizeByteBuf(PCMDateSize) As Byte
        fileSizeTotal = PCMDateSize + 44
        Dim wavH As waveHeaderStructre
        With wavH
            .RIFF = "RIFF"
            .FileSize = fileSizeTotal - 8
            .WAVE = "WAVE"
            .FORMAT = "fmt "
            .FormatSize = 16
            .FilePadding = 1
            .FormatChannels = channel
            .SamplesPerSecond = sampleHz
            .AverageBytesPerSecond = channel * sampleHz * channelBit / 8
            .BytesPerSample = channel * channelBit / 8
            .BitsPerSample = channelBit
            .DATA = "data"
            .DataSize = PCMDateSize
        End With
        Dim filename As String
        filename = "d:\" + TextBox5.Text
        Dim nFile = New FileStream(filename, FileMode.Create, FileAccess.ReadWrite)
        Dim bw As New BinaryWriter(nFile)
        For Each abc In wavH.GetType.GetFields
            bw.Write(abc.GetValue(wavH))
        Next
        Dim leftBuf As Byte
        Dim rightBuf As Byte
        Select Case channel
            Case 1
                For i = 0 To sampleHz * sampleTime - 1
                    leftBuf = Int(127 * Sin(2 * PI * happenHz * i / sampleHz) + 128) 'And &HFF

                    bw.Write(leftBuf)
                Next
            Case 2
                For i = 0 To sampleHz * sampleTime - 1
                    rightBuf = Int(127 * Sin(2 * PI * happenHz * i / sampleHz) + 128) 'And &HFF
                    bw.Write(rightBuf)
                    leftBuf = Int(127 * Cos(2 * PI * happenHz * i / sampleHz) + 128) 'And &HFF
                    bw.Write(leftBuf)
                Next
            Case Else
                MsgBox("对不起，目前只支持2个声道！")
                Return
        End Select
        bw.Flush()
        bw.Close()
        nFile.Close()

        MsgBox("finsh")
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles Me.Load
        PictureBox1.Width = 700

        PictureBox1.Height = 80
        PictureBox2.Width = 700
        PictureBox2.Height = 80

    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        Button4.Enabled = True
        bshowFilename = "D:\" + TextBox5.Text
        showWaveTrans()

    End Sub
    Private Sub showWaveTrans()
        Dim wavdata() As Byte
        Dim myFile As New IO.FileStream(bshowFilename, FileMode.Open, FileAccess.Read)
        Dim br As New BinaryReader(myFile)
        wavdata = br.ReadBytes(myFile.Length)
        Dim channel As String
        channel = System.BitConverter.ToUInt16(wavdata, 22)
        PictureBox1.Refresh()
        PictureBox2.Refresh()

        Select Case channel
            Case 1
                Dim data(wavdata.Length - 45) As Byte
                Array.ConstrainedCopy(wavdata, 44, data, 0, wavdata.Length - 44)
                '  MsgBox(Hex(data(0).ToString))
                showWave（PictureBox1, data)
            Case 2
                Dim leftData(Int((wavdata.Length - 45) / 2)) As Byte
                Dim rightData(Int((wavdata.Length - 45) / 2)) As Byte
                Dim ddata(wavdata.Length - 45) As Byte
                Array.ConstrainedCopy(wavdata, 44, ddata, 0, wavdata.Length - 44)
                Dim k As Integer
                k = 0
                For j = 0 To ddata.Length - 1 Step 2
                    leftData(k) = ddata(j)
                    rightData(k) = ddata(j + 1)
                    k = k + 1
                    'MsgBox("地址：" + Hex(44 + Int(j / 2)).ToString + "---" + Hex(leftData(j).ToString) + "---" + Hex(rightData(j).ToString))
                Next
                showWave（PictureBox1, leftData)
                showWave（PictureBox2, rightData)
            Case Else
                MsgBox("通道数超过2，本软件不能解析！")
        End Select


        br.Close()
        myFile.Close()

    End Sub
    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click

        OpenFileDialog1.ShowDialog()
        bshowFilename = OpenFileDialog1.FileName
        showWaveTrans()
        Button4.Enabled = True

    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        showWaveTrans()
    End Sub
End Class
