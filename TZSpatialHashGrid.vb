Public Class TZSpatialHashGrid
    Implements IDisposable

    Public Class Client
        Public Position As Vector3
        Public Bound_Min As Vector3
        Public Bound_Max As Vector3
        Public Indices(1) As Vector3
        Public Tag As Object
        Public Sub New(Position As Vector3, Bound_Min As Vector3, Bound_Max As Vector3, ByRef Tag As Object)
            Me.Position = Position
            Me.Bound_Min = Bound_Min
            Me.Bound_Max = Bound_Max
            Me.Tag = Tag
        End Sub
    End Class

    Public mHashTable As Dictionary(Of Integer, HashSet(Of Client))
    Private mGridSize As Integer
    Private mCellCount As Integer
    Private mCellCount_Squared As Integer
    Private mWorldSize As Integer
    Private mWorld_Min As Vector3
    Private mWorld_Max As Vector3
    Private mCellFactor As Single

    Public Sub New(World_Min As Vector3, World_Max As Vector3, Grid_Size As Integer)
        mGridSize = Grid_Size
        mWorld_Min = World_Min
        mWorld_Max = World_Max
        Dim Size As Vector3 = World_Max - World_Min
        Dim WorldSize As Integer = CInt(Math.Max(Math.Max(Size.X, Size.Y), Size.Z))

        mCellCount = WorldSize \ Grid_Size
        mCellCount_Squared = mCellCount * mCellCount
        mWorldSize = mCellCount * Grid_Size

        mCellFactor = CSng(1 / mGridSize)
        mHashTable = New Dictionary(Of Integer, HashSet(Of Client))

        Dim Min As Vector3 = Get_CellIndex(World_Min)
        Dim Max As Vector3 = Get_CellIndex(World_Max)
        Dim Current_Cell_Index As Integer
        For x As Integer = CInt(Min.X) To CInt(Max.X)
            For y As Integer = CInt(Min.Y) To CInt(Max.Y)
                For z As Integer = CInt(Min.Z) To CInt(Max.Z)
                    Current_Cell_Index = Cell_To_Index(x, y, z)
                    mHashTable(Current_Cell_Index) = New HashSet(Of Client)
                Next
            Next
        Next
    End Sub

    Public Function Position_To_Index(Position As Vector3) As Integer
        Return CInt(Math.Floor(Position.X * mCellFactor)) + CInt(Math.Floor(Position.Y * mCellFactor) * mCellCount) + CInt(Math.Floor(Position.Z * mCellFactor)) * mCellCount_Squared
    End Function

    Public Function Get_CellIndex(Position As Vector3) As Vector3
        Dim CellPos As Vector3
        CellPos.X = CInt(Math.Floor(Position.X * mCellFactor))
        CellPos.Y = CInt(Math.Floor(Position.Y * mCellFactor))
        CellPos.Z = CInt(Math.Floor(Position.Z * mCellFactor))

        Return CellPos
    End Function

    Public Function Cell_To_Index(X As Integer, Y As Integer, Z As Integer) As Integer
        Return CInt(X + (Y * mCellCount) + (Z * mCellCount_Squared))
    End Function

    Public Sub Add(myClient As Client)
        Dim Min As Vector3 = Get_CellIndex(Clamp_To_World(myClient.Position + myClient.Bound_Min))
        Dim Max As Vector3 = Get_CellIndex(Clamp_To_World(myClient.Position + myClient.Bound_Max))
        myClient.Indices(0) = Min
        myClient.Indices(1) = Max
        Dim currentCell As Integer

        For x As Integer = CInt(Min.X) To CInt(Max.X)
            For y As Integer = CInt(Min.Y) To CInt(Max.Y)
                For z As Integer = CInt(Min.Z) To CInt(Max.Z)
                    currentCell = Cell_To_Index(x, y, z)

                    mHashTable(currentCell).Add(myClient)
                Next
            Next
        Next
    End Sub

    Private Function Clamp_To_World(Position As Vector3) As Vector3
        Position = New Vector3(Math.Max(Position.X, mWorld_Min.X), Math.Max(Position.Y, mWorld_Min.Y), Math.Max(Position.Z, mWorld_Min.Z))
        Return New Vector3(Math.Min(Position.X, mWorld_Max.X), Math.Min(Position.Y, mWorld_Max.Y), Math.Min(Position.Z, mWorld_Max.Z))
    End Function

    Public Sub Remove(myClient As Client)
        Dim Min As Vector3 = myClient.Indices(0)
        Dim Max As Vector3 = myClient.Indices(1)
        Dim currentCell As Integer
        For x As Integer = CInt(Min.X) To CInt(Max.X)
            For y As Integer = CInt(Min.Y) To CInt(Max.Y)
                For z As Integer = CInt(Min.Z) To CInt(Max.Z)
                    currentCell = Cell_To_Index(x, y, z)
                    mHashTable(currentCell).Remove(myClient)
                Next
            Next
        Next
    End Sub

    Public Sub Update(myClient As Client)
        Dim Min As Vector3 = Get_CellIndex(myClient.Position + myClient.Bound_Min)
        Dim Max As Vector3 = Get_CellIndex(myClient.Position + myClient.Bound_Max)
        If Min = myClient.Indices(0) AndAlso Max = myClient.Indices(1) Then Exit Sub
        Remove(myClient)
        Add(myClient)
    End Sub

    Public Function Get_Client(myCell As Integer) As HashSet(Of Client)
        If Not mHashTable.ContainsKey(myCell) Then Return Nothing
        Return mHashTable(myCell)
    End Function

    Public Sub Get_Client(Position As Vector3, Radius As Single, ByRef result As HashSet(Of Client))
        Dim Min As Vector3 = New Vector3(Position.X - Radius, Position.Y - Radius, Position.Z - Radius)
        Dim Max As Vector3 = New Vector3(Position.X + Radius, Position.Y + Radius, Position.Z + Radius)

        Get_Client(Min, Max, result)
    End Sub

    Public Sub Get_Client(Min As Vector3, Max As Vector3, ByRef result As HashSet(Of Client))
        Dim currentCell As Integer
        Dim HashSetFound As HashSet(Of Client)

        Min = Get_CellIndex(Min)
        Max = Get_CellIndex(Max)

        For x As Integer = CInt(Min.X) To CInt(Max.X)
            For y As Integer = CInt(Min.Y) To CInt(Max.Y)
                For z As Integer = CInt(Min.Z) To CInt(Max.Z)
                    currentCell = Cell_To_Index(x, y, z)

                    If Not mHashTable.ContainsKey(currentCell) Then Continue For

                    HashSetFound = mHashTable(currentCell)
                    result.UnionWith(HashSetFound)
                Next
            Next
        Next
    End Sub

    ''' <summary>
    ''' Get client from a camera
    ''' </summary>
    ''' <param name="Position">Camera position</param>
    ''' <param name="Look">Camera Direction</param>
    ''' <param name="Znear">Near plane</param>
    ''' <param name="Zfar">Far plane</param>
    ''' <param name="Fov">Fov camera (in degree)</param>
    ''' <param name="AspectRatio">Camera aspect</param>
    ''' <param name="UpVector">Camera Up Vector (0,1,0)</param>
    ''' <param name="result">List of actors found</param>
    Public Sub Get_Client_Frustum(Position As Vector3, Look As Vector3, Znear As Single, Zfar As Single, Fov As Single, AspectRatio As Single, UpVector As Vector3, ByRef result As HashSet(Of Client))
        Dim CornerFrustum() As Vector3 = CalculateFrustumCorner(Position, Look, Znear, Zfar, Fov, AspectRatio, UpVector)
        Dim Min As Vector3 = New Vector3(Single.MaxValue, Single.MaxValue, Single.MaxValue)
        Dim Max As Vector3 = New Vector3(Single.MinValue, Single.MinValue, Single.MinValue)

        For i As Integer = 0 To CornerFrustum.Length - 1
            Min = New Vector3(Math.Min(Min.X, CornerFrustum(i).X), Math.Min(Min.Y, CornerFrustum(i).Y), Math.Min(Min.Z, CornerFrustum(i).Z))
            Max = New Vector3(Math.Max(Max.X, CornerFrustum(i).X), Math.Max(Max.Y, CornerFrustum(i).Y), Math.Max(Max.Z, CornerFrustum(i).Z))
        Next

        Get_Client(Min, Max, result)
    End Sub

    Private Shared Function CalculateFrustumCorner(Position As Vector3, Look As Vector3, Znear As Single, Zfar As Single, Fov As Single, AspectRatio As Single, UpVector As Vector3) As Vector3()
        Dim m_vaFrustumCorners(7) As Vector3

        Dim fScale, fNearPlaneHeight, fNearPlaneWidth, fFarPlaneHeight, fFarPlaneWidth As Single
        Dim vFarPlaneCenter, vNearPlaneCenter, vZ, vX, vY As Vector3

        vZ = Vector3.Normalize(Look)
        vX = Vector3.Normalize(Vector3.Cross(UpVector, vZ))
        vY = Vector3.Normalize(Vector3.Cross(vZ, vX))

        fNearPlaneHeight = CSng(Math.Tan(DegToRad(Fov) * 0.5F) * Znear)
        fNearPlaneWidth = fNearPlaneHeight * AspectRatio

        fFarPlaneHeight = CSng(Math.Tan(DegToRad(Fov) * 0.5F) * Zfar)
        fFarPlaneWidth = fFarPlaneHeight * AspectRatio

        vNearPlaneCenter = Position + vZ * Znear
        vFarPlaneCenter = Position + vZ * Zfar

        m_vaFrustumCorners(0) = vNearPlaneCenter - vX * fNearPlaneWidth - vY * fNearPlaneHeight
        m_vaFrustumCorners(1) = vNearPlaneCenter + vX * fNearPlaneWidth - vY * fNearPlaneHeight

        m_vaFrustumCorners(3) = vNearPlaneCenter - vX * fNearPlaneWidth + vY * fNearPlaneHeight
        m_vaFrustumCorners(2) = vNearPlaneCenter + vX * fNearPlaneWidth + vY * fNearPlaneHeight

        m_vaFrustumCorners(4) = vFarPlaneCenter - vX * fFarPlaneWidth - vY * fFarPlaneHeight
        m_vaFrustumCorners(5) = vFarPlaneCenter + vX * fFarPlaneWidth - vY * fFarPlaneHeight

        m_vaFrustumCorners(7) = vFarPlaneCenter - vX * fFarPlaneWidth + vY * fFarPlaneHeight
        m_vaFrustumCorners(6) = vFarPlaneCenter + vX * fFarPlaneWidth + vY * fFarPlaneHeight

        fScale = 1.1F

        Dim vcenter As Vector3
        For i As Integer = 0 To 7
            vcenter += m_vaFrustumCorners(i)
        Next
        vcenter /= 8.0F
        For i As Integer = 0 To 7
            m_vaFrustumCorners(i) += (m_vaFrustumCorners(i) - vcenter) * (fScale - 1.0F)
        Next

        Return m_vaFrustumCorners
    End Function

    Private Shared Function DegToRad(Deg As Single) As Single
        Return CSng(Math.PI * Deg / 180.0F)
    End Function

#Region "IDisposable Support"
    Private disposedValue As Boolean

    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then
                mHashTable.Clear()
                mHashTable = Nothing
            End If
        End If
        disposedValue = True
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
    End Sub
#End Region

End Class
