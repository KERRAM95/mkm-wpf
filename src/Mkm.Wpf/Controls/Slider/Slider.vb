Imports System.Windows

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' Le curseur de mkm (<c>Slider</c>) : une piste fine, colorée jusqu'à la valeur, et une poignée ronde qu'on tire.
    ''' <code>
    ''' &lt;mkm:Slider Minimum="0" Maximum="100" Value="40" /&gt;
    ''' </code>
    ''' </summary>
    ''' <remarks>
    ''' C'est le <see cref="System.Windows.Controls.Slider"/> de WPF : <c>Minimum</c>, <c>Maximum</c>, <c>Value</c>,
    ''' <c>SmallChange</c> (le pas des flèches), <c>LargeChange</c> (celui de Page haut et bas), <c>TickFrequency</c> et
    ''' <c>IsSnapToTickEnabled</c> (le pas de la souris, <c>step</c> chez mkm), <c>Orientation</c>, le clavier et les
    ''' lecteurs d'écran. « Slider » est aussi celui de WPF : dans un fichier VB qui importe <c>Mkm.Wpf.Controls</c>,
    ''' écrire l'un ou l'autre en entier.
    ''' </remarks>
    Public Class Slider
        Inherits System.Windows.Controls.Slider

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(Slider), New FrameworkPropertyMetadata(GetType(Slider)))
        End Sub

    End Class

End Namespace
