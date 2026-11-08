Imports System.Collections.Generic
Imports System.Windows
Imports System.Windows.Automation.Peers

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
            ' Le pas de mkm (step) : 1 ; celui de WPF est 0,1. Page haut et bas : 10.
            SmallChangeProperty.OverrideMetadata(GetType(Slider), New FrameworkPropertyMetadata(1.0))
            LargeChangeProperty.OverrideMetadata(GetType(Slider), New FrameworkPropertyMetadata(10.0))
            ' À la souris aussi, une valeur entière (mkm arrondit au pas) : la poignée s'arrête sur les graduations de 1.
            IsSnapToTickEnabledProperty.OverrideMetadata(GetType(Slider), New FrameworkPropertyMetadata(True))
        End Sub

        ''' <summary>Un curseur pour les lecteurs d'écran, avec sa valeur ; sa piste et sa poignée ne sont que son dessin.</summary>
        Protected Overrides Function OnCreateAutomationPeer() As AutomationPeer
            Return New PairDeSlider(Me)
        End Function

        Private NotInheritable Class PairDeSlider
            Inherits SliderAutomationPeer

            Public Sub New(curseur As Slider)
                MyBase.New(curseur)
            End Sub

            ' Les deux zones de la piste seraient des boutons sans nom.
            Protected Overrides Function GetChildrenCore() As List(Of AutomationPeer)
                Return Nothing
            End Function

        End Class

    End Class

End Namespace
