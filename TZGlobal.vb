Public Structure Vector3
    Public X As Single
    Public Y As Single
    Public Z As Single

    Public Sub New(X As Single, Y As Single, Z As Single)
        Me.X = X
        Me.Y = Y
        Me.Z = Z
    End Sub

    Public Shared Function Normalize(vec As Vector3) As Vector3
        Dim m As Single = CSng(Math.Sqrt(vec.X * vec.X + vec.Y * vec.Y + vec.Z * vec.Z))
        Return New Vector3(vec.X / m, vec.Y / m, vec.Z / m)
    End Function

    Public Shared Function Cross(left As Vector3, right As Vector3) As Vector3
        Return New Vector3((left.Y * right.Z) - (left.Z * right.Y), (left.Z * right.X) - (left.X * right.Z), (left.X * right.Y) - (left.Y * right.X))
    End Function

    Public Shared Operator +(left As Vector3, right As Vector3) As Vector3
        Return New Vector3(left.X + right.X, left.Y + right.Y, left.Z + right.Z)
    End Operator

    Public Shared Operator -(left As Vector3, right As Vector3) As Vector3
        Return New Vector3(left.X - right.X, left.Y - right.Y, left.Z - right.Z)
    End Operator

    Public Shared Operator *(left As Vector3, right As Vector3) As Vector3
        Return New Vector3(left.X * right.X, left.Y * right.Y, left.Z * right.Z)
    End Operator

    Public Shared Operator *(source As Vector3, value As Single) As Vector3
        Return New Vector3(source.X * value, source.Y * value, source.Z * value)
    End Operator

    Public Shared Operator /(source As Vector3, value As Single) As Vector3
        Return New Vector3(source.X / value, source.Y / value, source.Z / value)
    End Operator

    Public Shared Operator =(left As Vector3, right As Vector3) As Boolean
        If left.X = right.X AndAlso left.Y = right.Y AndAlso left.Z = right.Z Then Return True
        Return False
    End Operator

    Public Shared Operator <>(left As Vector3, right As Vector3) As Boolean
        If left.X <> right.X OrElse left.Y <> right.Y OrElse left.Z <> right.Z Then Return True
        Return False
    End Operator
End Structure

Module TZGlobal
    Private mRand As Random
    Private TimeMesure As Stopwatch
    Sub Main()
        mRand = New Random()
        TimeMesure = New Stopwatch
        TimeMesure.Start()

        Dim Search_Radius As Integer = 15
        Dim ITERATION As Integer = 1000
        Dim CLIENT_COUNT As Integer = 100000
        Dim Bound_Min As Vector3 = New Vector3(-1000, -1000, -1000)
        Dim Bound_Max As Vector3 = New Vector3(1000, 1000, 1000)
        Dim CellSize As Integer = 100


        Dim SHG As New TZSpatialHashGrid(Bound_Min, Bound_Max, CellSize)


        Dim Size As Vector3 = New Vector3(15, 15, 15)
        Dim BoundingBox_Min As Vector3 = New Vector3(-Size.X * 0.5F, -Size.Y * 0.5F, -Size.Z * 0.5F)
        Dim BoundingBox_Max As Vector3 = New Vector3(Size.X * 0.5F, Size.Y * 0.5F, Size.Z * 0.5F)

        Dim TimeStart As Long = TimeMesure.ElapsedMilliseconds
        Dim timeEnd As Long
        Dim clientPos As Vector3
        For i As Integer = 0 To CLIENT_COUNT - 1
            clientPos.X = mRand.Next(CInt(Bound_Min.X), CInt(Bound_Max.X))
            clientPos.Y = mRand.Next(CInt(Bound_Min.Y), CInt(Bound_Max.Y))
            clientPos.Z = mRand.Next(CInt(Bound_Min.Z), CInt(Bound_Max.Z))
            SHG.Add(New TZSpatialHashGrid.Client(clientPos, BoundingBox_Min, BoundingBox_Max, "Actor_" & i))
        Next
        timeEnd = TimeMesure.ElapsedMilliseconds
        System.Console.WriteLine("Add time: " & (timeEnd - TimeStart) & " ms")


        Dim RandSearch As Vector3
        Dim Min As Vector3
        Dim Max As Vector3

        Dim Result As New HashSet(Of TZSpatialHashGrid.Client)
        TimeStart = TimeMesure.ElapsedMilliseconds
        For i As Integer = 0 To ITERATION - 1
            RandSearch.X = mRand.Next(CInt(Bound_Min.X), CInt(Bound_Max.X))
            RandSearch.Y = mRand.Next(CInt(Bound_Min.Y), CInt(Bound_Max.Y))
            RandSearch.Z = mRand.Next(CInt(Bound_Min.Z), CInt(Bound_Max.Z))
            Min = New Vector3(RandSearch.X - Search_Radius, RandSearch.Y - Search_Radius, RandSearch.Z - Search_Radius)
            Max = New Vector3(RandSearch.X + Search_Radius, RandSearch.Y + Search_Radius, RandSearch.Z + Search_Radius)

            SHG.Get_Client(Min, Max, Result)

            '[...]do stuff with the founded client

            Result.Clear()
        Next
        timeEnd = TimeMesure.ElapsedMilliseconds

        System.Console.WriteLine("Search (" & ITERATION & " Iterations " & "" & ") : " & (timeEnd - TimeStart) & " ms")

        'single search
        RandSearch.X = mRand.Next(CInt(Bound_Min.X), CInt(Bound_Max.X))
        RandSearch.Y = mRand.Next(CInt(Bound_Min.Y), CInt(Bound_Max.Y))
        RandSearch.Z = mRand.Next(CInt(Bound_Min.Z), CInt(Bound_Max.Z))
        Min = New Vector3(RandSearch.X - Search_Radius, RandSearch.Y - Search_Radius, RandSearch.Z - Search_Radius)
        Max = New Vector3(RandSearch.X + Search_Radius, RandSearch.Y + Search_Radius, RandSearch.Z + Search_Radius)

        TimeStart = TimeMesure.ElapsedMilliseconds
        SHG.Get_Client(Min, Max, Result)
        timeEnd = TimeMesure.ElapsedMilliseconds

        System.Console.WriteLine("Search: " & (timeEnd - TimeStart) & " ms")
        System.Console.WriteLine("Actor Found: " & Result.Count)

        System.Console.ReadKey()
    End Sub

End Module
