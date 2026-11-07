Imports System.Windows
Imports System.Windows.Controls

Namespace Mkm.Wpf.Controls

    ''' <summary>Le trait qui sépare deux groupes de commandes d'une <see cref="ActionBar"/>.</summary>
    Public Class ActionBarSeparator
        Inherits Control

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(ActionBarSeparator), New FrameworkPropertyMetadata(GetType(ActionBarSeparator)))
            FocusableProperty.OverrideMetadata(GetType(ActionBarSeparator), New FrameworkPropertyMetadata(False))
            IsTabStopProperty.OverrideMetadata(GetType(ActionBarSeparator), New FrameworkPropertyMetadata(False))
        End Sub

    End Class

End Namespace
