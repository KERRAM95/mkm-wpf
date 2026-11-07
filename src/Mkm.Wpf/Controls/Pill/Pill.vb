Imports System.Windows
Imports System.Windows.Controls

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' Une pilule : un cadre aux bouts tout ronds, quelle que soit sa taille — le rayon de 999 px de mkm, que WPF ne borne
    ''' pas comme CSS (il dessinerait un fuseau) : ici, la moitié du plus petit côté.
    ''' </summary>
    Public Class Pill
        Inherits Border

        ''' <inheritdoc/>
        Protected Overrides Sub OnRenderSizeChanged(sizeInfo As SizeChangedInfo)
            MyBase.OnRenderSizeChanged(sizeInfo)
            CornerRadius = New CornerRadius(Math.Min(sizeInfo.NewSize.Width, sizeInfo.NewSize.Height) / 2)
        End Sub

    End Class

End Namespace
