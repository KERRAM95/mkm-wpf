' Les icônes de mkm, dessinées par Tabler Icons (MIT, voir THIRD-PARTY-NOTICES.md) :
' @tabler/icons 3.48.0, style outline, sur la grille de 24 de Tabler.
' Fichier produit par outils/icones/importer-tabler.mjs : ne pas le modifier à la main.
Imports System.Windows.Media

Namespace Mkm.Wpf.Controls

    Partial Public NotInheritable Class Icons

        ''' <summary>« add » : plus (Tabler).</summary>
        Public Shared ReadOnly Property Add As Geometry = Dessin("M 12 5 l 0 14 M 5 12 l 14 0")

        ''' <summary>« attach » : link-plus (Tabler).</summary>
        Public Shared ReadOnly Property Attach As Geometry = Dessin("M 9 15 l 6 -6 M 11 6 l 0.463 -0.536 a 5 5 0 0 1 7.072 0 a 4.993 4.993 0 0 1 -0.001 7.072 M 12.603 18.534 a 5.07 5.07 0 0 1 -7.127 0 a 4.972 4.972 0 0 1 0 -7.071 l 0.524 -0.463 M 16 19 h 6 M 19 16 v 6")

        ''' <summary>« attachment » : paperclip (Tabler).</summary>
        Public Shared ReadOnly Property Attachment As Geometry = Dessin("M 15 7 l -6.5 6.5 a 1.5 1.5 0 0 0 3 3 l 6.5 -6.5 a 3 3 0 0 0 -6 -6 l -6.5 6.5 a 4.5 4.5 0 0 0 9 9 l 6.5 -6.5")

        ''' <summary>« back » : arrow-left (Tabler).</summary>
        Public Shared ReadOnly Property Back As Geometry = Dessin("M 5 12 l 14 0 M 5 12 l 6 6 M 5 12 l 6 -6")

        ''' <summary>« blocked » : ban (Tabler).</summary>
        Public Shared ReadOnly Property Blocked As Geometry = Dessin("M 3 12 a 9 9 0 1 0 18 0 a 9 9 0 1 0 -18 0 M 5.7 5.7 l 12.6 12.6")

        ''' <summary>« caret-down » : caret-down (Tabler).</summary>
        Public Shared ReadOnly Property CaretDown As Geometry = Dessin("M 6 10 l 6 6 l 6 -6 h -12")

        ''' <summary>« caret-right » : caret-right (Tabler).</summary>
        Public Shared ReadOnly Property CaretRight As Geometry = Dessin("M 10 18 l 6 -6 l -6 -6 v 12")

        ''' <summary>« check » : check (Tabler).</summary>
        Public Shared ReadOnly Property Check As Geometry = Dessin("M 5 12 l 5 5 l 10 -10")

        ''' <summary>« chevron-down » : chevron-down (Tabler).</summary>
        Public Shared ReadOnly Property ChevronDown As Geometry = Dessin("M 6 9 l 6 6 l 6 -6")

        ''' <summary>« chevron-left » : chevron-left (Tabler).</summary>
        Public Shared ReadOnly Property ChevronLeft As Geometry = Dessin("M 15 6 l -6 6 l 6 6")

        ''' <summary>« chevron-right » : chevron-right (Tabler).</summary>
        Public Shared ReadOnly Property ChevronRight As Geometry = Dessin("M 9 6 l 6 6 l -6 6")

        ''' <summary>« chevron-up » : chevron-up (Tabler).</summary>
        Public Shared ReadOnly Property ChevronUp As Geometry = Dessin("M 6 15 l 6 -6 l 6 6")

        ''' <summary>« close » : x (Tabler).</summary>
        Public Shared ReadOnly Property Close As Geometry = Dessin("M 18 6 l -12 12 M 6 6 l 12 12")

        ''' <summary>« collapse-all » : chevrons-up (Tabler).</summary>
        Public Shared ReadOnly Property CollapseAll As Geometry = Dessin("M 7 11 l 5 -5 l 5 5 M 7 17 l 5 -5 l 5 5")

        ''' <summary>« column-fit » : arrow-autofit-width (Tabler).</summary>
        Public Shared ReadOnly Property ColumnFit As Geometry = Dessin("M 4 12 v -6 a 2 2 0 0 1 2 -2 h 12 a 2 2 0 0 1 2 2 v 6 M 10 18 h -7 M 21 18 h -7 M 6 15 l -3 3 l 3 3 M 18 15 l 3 3 l -3 3")

        ''' <summary>« column-fit-all » : arrow-autofit-content (Tabler).</summary>
        Public Shared ReadOnly Property ColumnFitAll As Geometry = Dessin("M 6 4 l -3 3 l 3 3 M 18 4 l 3 3 l -3 3 M 4 16 a 2 2 0 0 1 2 -2 h 12 a 2 2 0 0 1 2 2 v 2 a 2 2 0 0 1 -2 2 h -12 a 2 2 0 0 1 -2 -2 l 0 -2 M 10 7 h -7 M 21 7 h -7")

        ''' <summary>« copy » : copy (Tabler).</summary>
        Public Shared ReadOnly Property Copy As Geometry = Dessin("M 7 9.667 a 2.667 2.667 0 0 1 2.667 -2.667 h 8.666 a 2.667 2.667 0 0 1 2.667 2.667 v 8.666 a 2.667 2.667 0 0 1 -2.667 2.667 h -8.666 a 2.667 2.667 0 0 1 -2.667 -2.667 l 0 -8.666 M 4.012 16.737 a 2.005 2.005 0 0 1 -1.012 -1.737 v -10 c 0 -1.1 0.9 -2 2 -2 h 10 c 0.75 0 1.158 0.385 1.5 1")

        ''' <summary>« delete » : trash (Tabler).</summary>
        Public Shared ReadOnly Property Delete As Geometry = Dessin("M 4 7 l 16 0 M 10 11 l 0 6 M 14 11 l 0 6 M 5 7 l 1 12 a 2 2 0 0 0 2 2 h 8 a 2 2 0 0 0 2 -2 l 1 -12 M 9 7 v -3 a 1 1 0 0 1 1 -1 h 4 a 1 1 0 0 1 1 1 v 3")

        ''' <summary>« detach » : unlink (Tabler).</summary>
        Public Shared ReadOnly Property Detach As Geometry = Dessin("M 17 22 v -2 M 9 15 l 6 -6 M 11 6 l 0.463 -0.536 a 5 5 0 0 1 7.071 7.072 l -0.534 0.464 M 13 18 l -0.397 0.534 a 5.068 5.068 0 0 1 -7.127 0 a 4.972 4.972 0 0 1 0 -7.071 l 0.524 -0.463 M 20 17 h 2 M 2 7 h 2 M 7 2 v 2")

        ''' <summary>« document » : file-text (Tabler).</summary>
        Public Shared ReadOnly Property Document As Geometry = Dessin("M 14 3 v 4 a 1 1 0 0 0 1 1 h 4 M 17 21 h -10 a 2 2 0 0 1 -2 -2 v -14 a 2 2 0 0 1 2 -2 h 7 l 5 5 v 11 a 2 2 0 0 1 -2 2 M 9 9 l 1 0 M 9 13 l 6 0 M 9 17 l 6 0")

        ''' <summary>« documents » : files (Tabler).</summary>
        Public Shared ReadOnly Property Documents As Geometry = Dessin("M 15 3 v 4 a 1 1 0 0 0 1 1 h 4 M 18 17 h -7 a 2 2 0 0 1 -2 -2 v -10 a 2 2 0 0 1 2 -2 h 4 l 5 5 v 7 a 2 2 0 0 1 -2 2 M 16 17 v 2 a 2 2 0 0 1 -2 2 h -7 a 2 2 0 0 1 -2 -2 v -10 a 2 2 0 0 1 2 -2 h 2")

        ''' <summary>« download » : download (Tabler).</summary>
        Public Shared ReadOnly Property Download As Geometry = Dessin("M 4 17 v 2 a 2 2 0 0 0 2 2 h 12 a 2 2 0 0 0 2 -2 v -2 M 7 11 l 5 5 l 5 -5 M 12 4 l 0 12")

        ''' <summary>« drawings » : file-vector (Tabler).</summary>
        Public Shared ReadOnly Property Drawings As Geometry = Dessin("M 14 3 v 4 a 1 1 0 0 0 1 1 h 4 M 8 16.5 a 1.5 1.5 0 1 0 3 0 a 1.5 1.5 0 1 0 -3 0 M 13 12.5 a 1.5 1.5 0 1 0 3 0 a 1.5 1.5 0 1 0 -3 0 M 17 21 h -10 a 2 2 0 0 1 -2 -2 v -14 a 2 2 0 0 1 2 -2 h 7 l 5 5 v 11 a 2 2 0 0 1 -2 2 M 9.5 15 a 2.5 2.5 0 0 1 2.5 -2.5 h 1")

        ''' <summary>« drop » : drag-drop (Tabler).</summary>
        Public Shared ReadOnly Property Drop As Geometry = Dessin("M 19 11 v -2 a 2 2 0 0 0 -2 -2 h -8 a 2 2 0 0 0 -2 2 v 8 a 2 2 0 0 0 2 2 h 2 M 13 13 l 9 3 l -4 2 l -2 4 l -3 -9 M 3 3 l 0 0.01 M 7 3 l 0 0.01 M 11 3 l 0 0.01 M 15 3 l 0 0.01 M 3 7 l 0 0.01 M 3 11 l 0 0.01 M 3 15 l 0 0.01")

        ''' <summary>« edit » : pencil (Tabler).</summary>
        Public Shared ReadOnly Property Edit As Geometry = Dessin("M 4 20 h 4 l 10.5 -10.5 a 2.828 2.828 0 1 0 -4 -4 l -10.5 10.5 v 4 M 13.5 6.5 l 4 4")

        ''' <summary>« error » : circle-x (Tabler).</summary>
        Public Shared ReadOnly Property [Error] As Geometry = Dessin("M 3 12 a 9 9 0 1 0 18 0 a 9 9 0 1 0 -18 0 M 10 10 l 4 4 m 0 -4 l -4 4")

        ''' <summary>« expand-all » : chevrons-down (Tabler).</summary>
        Public Shared ReadOnly Property ExpandAll As Geometry = Dessin("M 7 7 l 5 5 l 5 -5 M 7 13 l 5 5 l 5 -5")

        ''' <summary>« export » : file-export (Tabler).</summary>
        Public Shared ReadOnly Property Export As Geometry = Dessin("M 14 3 v 4 a 1 1 0 0 0 1 1 h 4 M 11.5 21 h -4.5 a 2 2 0 0 1 -2 -2 v -14 a 2 2 0 0 1 2 -2 h 7 l 5 5 v 5 m -5 6 h 7 m -3 -3 l 3 3 l -3 3")

        ''' <summary>« files » : folder-open (Tabler).</summary>
        Public Shared ReadOnly Property Files As Geometry = Dessin("M 5 19 l 2.757 -7.351 a 1 1 0 0 1 0.936 -0.649 h 12.307 a 1 1 0 0 1 0.986 1.164 l -0.996 5.211 a 2 2 0 0 1 -1.964 1.625 h -14.026 a 2 2 0 0 1 -2 -2 v -11 a 2 2 0 0 1 2 -2 h 4 l 3 3 h 7 a 2 2 0 0 1 2 2 v 2")

        ''' <summary>« filter » : filter (Tabler).</summary>
        Public Shared ReadOnly Property Filter As Geometry = Dessin("M 4 4 h 16 v 2.172 a 2 2 0 0 1 -0.586 1.414 l -4.414 4.414 v 7 l -6 2 v -8.5 l -4.48 -4.928 a 2 2 0 0 1 -0.52 -1.345 v -2.227")

        ''' <summary>« fit » : zoom-scan (Tabler).</summary>
        Public Shared ReadOnly Property Fit As Geometry = Dessin("M 8 11 a 3 3 0 1 0 6 0 a 3 3 0 0 0 -6 0 M 16 16 l -2.5 -2.5 M 3 7 v -2 a 2 2 0 0 1 2 -2 h 2 M 3 17 v 2 a 2 2 0 0 0 2 2 h 2 M 17 3 h 2 a 2 2 0 0 1 2 2 v 2 M 17 21 h 2 a 2 2 0 0 0 2 -2 v -2")

        ''' <summary>« forward » : arrow-right (Tabler).</summary>
        Public Shared ReadOnly Property Forward As Geometry = Dessin("M 5 12 l 14 0 M 13 18 l 6 -6 M 13 6 l 6 6")

        ''' <summary>« hide » : eye-off (Tabler).</summary>
        Public Shared ReadOnly Property Hide As Geometry = Dessin("M 10.585 10.587 a 2 2 0 0 0 2.829 2.828 M 16.681 16.673 a 8.717 8.717 0 0 1 -4.681 1.327 c -3.6 0 -6.6 -2 -9 -6 c 1.272 -2.12 2.712 -3.678 4.32 -4.674 m 2.86 -1.146 a 9.055 9.055 0 0 1 1.82 -0.18 c 3.6 0 6.6 2 9 6 c -0.666 1.11 -1.379 2.067 -2.138 2.87 M 3 3 l 18 18")

        ''' <summary>« history » : history (Tabler).</summary>
        Public Shared ReadOnly Property History As Geometry = Dessin("M 12 8 l 0 4 l 2 2 M 3.05 11 a 9 9 0 1 1 0.5 4 m -0.5 5 v -5 h 5")

        ''' <summary>« home » : home (Tabler).</summary>
        Public Shared ReadOnly Property Home As Geometry = Dessin("M 5 12 l -2 0 l 9 -9 l 9 9 l -2 0 M 5 12 v 7 a 2 2 0 0 0 2 2 h 10 a 2 2 0 0 0 2 -2 v -7 M 9 21 v -6 a 2 2 0 0 1 2 -2 h 2 a 2 2 0 0 1 2 2 v 6")

        ''' <summary>« import » : file-import (Tabler).</summary>
        Public Shared ReadOnly Property Import As Geometry = Dessin("M 14 3 v 4 a 1 1 0 0 0 1 1 h 4 M 5 13 v -8 a 2 2 0 0 1 2 -2 h 7 l 5 5 v 11 a 2 2 0 0 1 -2 2 h -5.5 m -9.5 -2 h 7 m -3 -3 l 3 3 l -3 3")

        ''' <summary>« info » : info-circle (Tabler).</summary>
        Public Shared ReadOnly Property Info As Geometry = Dessin("M 3 12 a 9 9 0 1 0 18 0 a 9 9 0 0 0 -18 0 M 12 9 h 0.01 M 11 12 h 1 v 4 h 1")

        ''' <summary>« insert-item » : cube-plus (Tabler).</summary>
        Public Shared ReadOnly Property InsertItem As Geometry = Dessin("M 21 12.5 v -4.509 a 1.98 1.98 0 0 0 -1 -1.717 l -7 -4.008 a 2.016 2.016 0 0 0 -2 0 l -7 4.007 c -0.619 0.355 -1 1.01 -1 1.718 v 8.018 c 0 0.709 0.381 1.363 1 1.717 l 7 4.008 a 2.016 2.016 0 0 0 2 0 M 12 22 v -10 M 12 12 l 8.73 -5.04 M 3.27 6.96 l 8.73 5.04 M 16 19 h 6 M 19 16 v 6")

        ''' <summary>« launch » : player-play (Tabler).</summary>
        Public Shared ReadOnly Property Launch As Geometry = Dessin("M 7 4 v 16 l 13 -8 l -13 -8")

        ''' <summary>« link » : link (Tabler).</summary>
        Public Shared ReadOnly Property Link As Geometry = Dessin("M 9 15 l 6 -6 M 11 6 l 0.463 -0.536 a 5 5 0 0 1 7.071 7.072 l -0.534 0.464 M 13 18 l -0.397 0.534 a 5.068 5.068 0 0 1 -7.127 0 a 4.972 4.972 0 0 1 0 -7.071 l 0.524 -0.463")

        ''' <summary>« list » : list (Tabler).</summary>
        Public Shared ReadOnly Property List As Geometry = Dessin("M 9 6 l 11 0 M 9 12 l 11 0 M 9 18 l 11 0 M 5 6 l 0 0.01 M 5 12 l 0 0.01 M 5 18 l 0 0.01")

        ''' <summary>« loading » : loader-2 (Tabler).</summary>
        Public Shared ReadOnly Property Loading As Geometry = Dessin("M 12 3 a 9 9 0 1 0 9 9")

        ''' <summary>« locked » : lock (Tabler).</summary>
        Public Shared ReadOnly Property Locked As Geometry = Dessin("M 5 13 a 2 2 0 0 1 2 -2 h 10 a 2 2 0 0 1 2 2 v 6 a 2 2 0 0 1 -2 2 h -10 a 2 2 0 0 1 -2 -2 v -6 M 11 16 a 1 1 0 1 0 2 0 a 1 1 0 0 0 -2 0 M 8 11 v -4 a 4 4 0 1 1 8 0 v 4")

        ''' <summary>« maturity » : progress-check (Tabler).</summary>
        Public Shared ReadOnly Property Maturity As Geometry = Dessin("M 10 20.777 a 8.942 8.942 0 0 1 -2.48 -0.969 M 14 3.223 a 9.003 9.003 0 0 1 0 17.554 M 4.579 17.093 a 8.961 8.961 0 0 1 -1.227 -2.592 M 3.124 10.5 c 0.16 -0.95 0.468 -1.85 0.9 -2.675 l 0.169 -0.305 M 6.907 4.579 a 8.954 8.954 0 0 1 3.093 -1.356 M 9 12 l 2 2 l 4 -4")

        ''' <summary>« menu » : menu-2 (Tabler).</summary>
        Public Shared ReadOnly Property Menu As Geometry = Dessin("M 4 6 l 16 0 M 4 12 l 16 0 M 4 18 l 16 0")

        ''' <summary>« merge » : arrow-merge (Tabler).</summary>
        Public Shared ReadOnly Property Merge As Geometry = Dessin("M 8 7 l 4 -4 l 4 4 M 12 3 v 5.394 a 6.737 6.737 0 0 1 -3 5.606 a 6.737 6.737 0 0 0 -3 5.606 v 1.394 M 12 3 v 5.394 a 6.737 6.737 0 0 0 3 5.606 a 6.737 6.737 0 0 1 3 5.606 v 1.394")

        ''' <summary>« monitoring » : activity (Tabler).</summary>
        Public Shared ReadOnly Property Monitoring As Geometry = Dessin("M 3 12 h 4 l 3 8 l 4 -16 l 3 8 h 4")

        ''' <summary>« more » : dots-vertical (Tabler).</summary>
        Public Shared ReadOnly Property More As Geometry = Dessin("M 11 12 a 1 1 0 1 0 2 0 a 1 1 0 1 0 -2 0 M 11 19 a 1 1 0 1 0 2 0 a 1 1 0 1 0 -2 0 M 11 5 a 1 1 0 1 0 2 0 a 1 1 0 1 0 -2 0")

        ''' <summary>« move-down » : arrow-down (Tabler).</summary>
        Public Shared ReadOnly Property MoveDown As Geometry = Dessin("M 12 5 l 0 14 M 18 13 l -6 6 M 6 13 l 6 6")

        ''' <summary>« move-up » : arrow-up (Tabler).</summary>
        Public Shared ReadOnly Property MoveUp As Geometry = Dessin("M 12 5 l 0 14 M 18 11 l -6 -6 M 6 11 l 6 -6")

        ''' <summary>« open » : arrow-up-right (Tabler).</summary>
        Public Shared ReadOnly Property Open As Geometry = Dessin("M 17 7 l -10 10 M 8 7 l 9 0 l 0 9")

        ''' <summary>« pending » : clock (Tabler).</summary>
        Public Shared ReadOnly Property Pending As Geometry = Dessin("M 3 12 a 9 9 0 1 0 18 0 a 9 9 0 0 0 -18 0 M 12 7 v 5 l 3 3")

        ''' <summary>« people » : users (Tabler).</summary>
        Public Shared ReadOnly Property People As Geometry = Dessin("M 5 7 a 4 4 0 1 0 8 0 a 4 4 0 1 0 -8 0 M 3 21 v -2 a 4 4 0 0 1 4 -4 h 4 a 4 4 0 0 1 4 4 v 2 M 16 3.13 a 4 4 0 0 1 0 7.75 M 21 21 v -2 a 4 4 0 0 0 -3 -3.85")

        ''' <summary>« person » : user (Tabler).</summary>
        Public Shared ReadOnly Property Person As Geometry = Dessin("M 8 7 a 4 4 0 1 0 8 0 a 4 4 0 0 0 -8 0 M 6 21 v -2 a 4 4 0 0 1 4 -4 h 4 a 4 4 0 0 1 4 4 v 2")

        ''' <summary>« pin » : pinned (Tabler).</summary>
        Public Shared ReadOnly Property Pin As Geometry = Dessin("M 9 4 v 6 l -2 4 v 2 h 10 v -2 l -2 -4 v -6 M 12 16 l 0 5 M 8 4 l 8 0")

        ''' <summary>« pin-left » : arrow-bar-to-left (Tabler).</summary>
        Public Shared ReadOnly Property PinLeft As Geometry = Dessin("M 10 12 l 10 0 M 10 12 l 4 4 M 10 12 l 4 -4 M 4 4 l 0 16")

        ''' <summary>« pin-right » : arrow-bar-to-right (Tabler).</summary>
        Public Shared ReadOnly Property PinRight As Geometry = Dessin("M 14 12 l -10 0 M 14 12 l -4 4 M 14 12 l -4 -4 M 20 4 l 0 16")

        ''' <summary>« platform » : cloud (Tabler).</summary>
        Public Shared ReadOnly Property Platform As Geometry = Dessin("M 6.657 18 c -2.572 0 -4.657 -2.007 -4.657 -4.483 c 0 -2.475 2.085 -4.482 4.657 -4.482 c 0.393 -1.762 1.794 -3.2 3.675 -3.773 c 1.88 -0.572 3.956 -0.193 5.444 1 c 1.488 1.19 2.162 3.007 1.77 4.769 h 0.99 c 1.913 0 3.464 1.56 3.464 3.486 c 0 1.927 -1.551 3.487 -3.465 3.487 h -11.878")

        ''' <summary>« platform-check » : cloud-search (Tabler).</summary>
        Public Shared ReadOnly Property PlatformCheck As Geometry = Dessin("M 11 18.004 h -4.343 c -2.572 -0.004 -4.657 -2.011 -4.657 -4.487 c 0 -2.475 2.085 -4.482 4.657 -4.482 c 0.393 -1.762 1.794 -3.2 3.675 -3.773 c 1.88 -0.572 3.956 -0.193 5.444 1 c 1.488 1.19 2.162 3.007 1.77 4.769 h 0.99 M 15 18 a 3 3 0 1 0 6 0 a 3 3 0 1 0 -6 0 M 20.2 20.2 l 1.8 1.8")

        ''' <summary>« properties » : list-details (Tabler).</summary>
        Public Shared ReadOnly Property Properties As Geometry = Dessin("M 13 5 h 8 M 13 9 h 5 M 13 15 h 8 M 13 19 h 5 M 3 5 a 1 1 0 0 1 1 -1 h 4 a 1 1 0 0 1 1 1 v 4 a 1 1 0 0 1 -1 1 h -4 a 1 1 0 0 1 -1 -1 l 0 -4 M 3 15 a 1 1 0 0 1 1 -1 h 4 a 1 1 0 0 1 1 1 v 4 a 1 1 0 0 1 -1 1 h -4 a 1 1 0 0 1 -1 -1 l 0 -4")

        ''' <summary>« refresh » : refresh (Tabler).</summary>
        Public Shared ReadOnly Property Refresh As Geometry = Dessin("M 20 11 a 8.1 8.1 0 0 0 -15.5 -2 m -0.5 -4 v 4 h 4 M 4 13 a 8.1 8.1 0 0 0 15.5 2 m 0.5 4 v -4 h -4")

        ''' <summary>« relaunch » : rotate-clockwise (Tabler).</summary>
        Public Shared ReadOnly Property Relaunch As Geometry = Dessin("M 4.05 11 a 8 8 0 1 1 0.5 4 m -0.5 5 v -5 h 5")

        ''' <summary>« remove » : minus (Tabler).</summary>
        Public Shared ReadOnly Property Remove As Geometry = Dessin("M 5 12 l 14 0")

        ''' <summary>« remove-item » : cube-off (Tabler).</summary>
        Public Shared ReadOnly Property RemoveItem As Geometry = Dessin("M 20.83 16.809 c 0.11 -0.248 0.17 -0.52 0.17 -0.801 v -8.018 a 1.98 1.98 0 0 0 -1 -1.717 l -7 -4.008 a 2.016 2.016 0 0 0 -2 0 l -3.012 1.725 m -2.547 1.458 l -1.441 0.825 c -0.619 0.355 -1 1.01 -1 1.718 v 8.018 c 0 0.709 0.381 1.363 1 1.717 l 7 4.008 a 2.016 2.016 0 0 0 2 0 l 5.544 -3.174 M 12 22 v -10 M 14.532 10.538 l 6.198 -3.578 M 3.27 6.96 l 8.73 5.04 M 3 3 l 18 18")

        ''' <summary>« rename » : cursor-text (Tabler).</summary>
        Public Shared ReadOnly Property Rename As Geometry = Dessin("M 10 12 h 4 M 9 4 a 3 3 0 0 1 3 3 v 10 a 3 3 0 0 1 -3 3 M 15 4 a 3 3 0 0 0 -3 3 v 10 a 3 3 0 0 0 3 3")

        ''' <summary>« replace-item » : replace (Tabler).</summary>
        Public Shared ReadOnly Property ReplaceItem As Geometry = Dessin("M 3 4 a 1 1 0 0 1 1 -1 h 4 a 1 1 0 0 1 1 1 v 4 a 1 1 0 0 1 -1 1 h -4 a 1 1 0 0 1 -1 -1 l 0 -4 M 15 16 a 1 1 0 0 1 1 -1 h 4 a 1 1 0 0 1 1 1 v 4 a 1 1 0 0 1 -1 1 h -4 a 1 1 0 0 1 -1 -1 l 0 -4 M 21 11 v -3 a 2 2 0 0 0 -2 -2 h -6 l 3 3 m 0 -6 l -3 3 M 3 13 v 3 a 2 2 0 0 0 2 2 h 6 l -3 -3 m 0 6 l 3 -3")

        ''' <summary>« revision » : versions (Tabler).</summary>
        Public Shared ReadOnly Property Revision As Geometry = Dessin("M 10 7 a 2 2 0 0 1 2 -2 h 6 a 2 2 0 0 1 2 2 v 10 a 2 2 0 0 1 -2 2 h -6 a 2 2 0 0 1 -2 -2 l 0 -10 M 7 7 l 0 10 M 4 8 l 0 8")

        ''' <summary>« search » : search (Tabler).</summary>
        Public Shared ReadOnly Property Search As Geometry = Dessin("M 3 10 a 7 7 0 1 0 14 0 a 7 7 0 1 0 -14 0 M 21 21 l -6 -6")

        ''' <summary>« select-all » : square-check (Tabler).</summary>
        Public Shared ReadOnly Property SelectAll As Geometry = Dessin("M 3 5 a 2 2 0 0 1 2 -2 h 14 a 2 2 0 0 1 2 2 v 14 a 2 2 0 0 1 -2 2 h -14 a 2 2 0 0 1 -2 -2 v -14 M 9 12 l 2 2 l 4 -4")

        ''' <summary>« select-none » : square (Tabler).</summary>
        Public Shared ReadOnly Property SelectNone As Geometry = Dessin("M 3 5 a 2 2 0 0 1 2 -2 h 14 a 2 2 0 0 1 2 2 v 14 a 2 2 0 0 1 -2 2 h -14 a 2 2 0 0 1 -2 -2 v -14")

        ''' <summary>« settings » : settings (Tabler).</summary>
        Public Shared ReadOnly Property Settings As Geometry = Dessin("M 10.325 4.317 c 0.426 -1.756 2.924 -1.756 3.35 0 a 1.724 1.724 0 0 0 2.573 1.066 c 1.543 -0.94 3.31 0.826 2.37 2.37 a 1.724 1.724 0 0 0 1.065 2.572 c 1.756 0.426 1.756 2.924 0 3.35 a 1.724 1.724 0 0 0 -1.066 2.573 c 0.94 1.543 -0.826 3.31 -2.37 2.37 a 1.724 1.724 0 0 0 -2.572 1.065 c -0.426 1.756 -2.924 1.756 -3.35 0 a 1.724 1.724 0 0 0 -2.573 -1.066 c -1.543 0.94 -3.31 -0.826 -2.37 -2.37 a 1.724 1.724 0 0 0 -1.065 -2.572 c -1.756 -0.426 -1.756 -2.924 0 -3.35 a 1.724 1.724 0 0 0 1.066 -2.573 c -0.94 -1.543 0.826 -3.31 2.37 -2.37 c 1 0.608 2.296 0.07 2.572 -1.065 M 9 12 a 3 3 0 1 0 6 0 a 3 3 0 0 0 -6 0")

        ''' <summary>« show » : eye (Tabler).</summary>
        Public Shared ReadOnly Property Show As Geometry = Dessin("M 10 12 a 2 2 0 1 0 4 0 a 2 2 0 0 0 -4 0 M 21 12 c -2.4 4 -5.4 6 -9 6 c -3.6 0 -6.6 -2 -9 -6 c 2.4 -4 5.4 -6 9 -6 c 3.6 0 6.6 2 9 6")

        ''' <summary>« side-panel » : layout-sidebar-right (Tabler).</summary>
        Public Shared ReadOnly Property SidePanel As Geometry = Dessin("M 4 6 a 2 2 0 0 1 2 -2 h 12 a 2 2 0 0 1 2 2 v 12 a 2 2 0 0 1 -2 2 h -12 a 2 2 0 0 1 -2 -2 l 0 -12 M 15 4 l 0 16")

        ''' <summary>« sort » : arrows-sort (Tabler).</summary>
        Public Shared ReadOnly Property Sort As Geometry = Dessin("M 3 9 l 4 -4 l 4 4 m -4 -4 v 14 M 21 15 l -4 4 l -4 -4 m 4 4 v -14")

        ''' <summary>« sort-ascending » : sort-ascending (Tabler).</summary>
        Public Shared ReadOnly Property SortAscending As Geometry = Dessin("M 4 6 l 7 0 M 4 12 l 7 0 M 4 18 l 9 0 M 15 9 l 3 -3 l 3 3 M 18 6 l 0 12")

        ''' <summary>« sort-descending » : sort-descending (Tabler).</summary>
        Public Shared ReadOnly Property SortDescending As Geometry = Dessin("M 4 6 l 9 0 M 4 12 l 7 0 M 4 18 l 7 0 M 15 15 l 3 3 l 3 -3 M 18 6 l 0 12")

        ''' <summary>« split » : arrows-split-2 (Tabler).</summary>
        Public Shared ReadOnly Property Split As Geometry = Dessin("M 21 17 h -5.397 a 5 5 0 0 1 -4.096 -2.133 l -0.514 -0.734 a 5 5 0 0 0 -4.096 -2.133 h -3.897 M 21 7 h -5.395 a 5 5 0 0 0 -4.098 2.135 l -0.51 0.73 a 5 5 0 0 1 -4.097 2.135 h -3.9 M 18 10 l 3 -3 l -3 -3 M 18 20 l 3 -3 l -3 -3")

        ''' <summary>« statistics » : chart-bar (Tabler).</summary>
        Public Shared ReadOnly Property Statistics As Geometry = Dessin("M 3 13 a 1 1 0 0 1 1 -1 h 4 a 1 1 0 0 1 1 1 v 6 a 1 1 0 0 1 -1 1 h -4 a 1 1 0 0 1 -1 -1 l 0 -6 M 15 9 a 1 1 0 0 1 1 -1 h 4 a 1 1 0 0 1 1 1 v 10 a 1 1 0 0 1 -1 1 h -4 a 1 1 0 0 1 -1 -1 l 0 -10 M 9 5 a 1 1 0 0 1 1 -1 h 4 a 1 1 0 0 1 1 1 v 14 a 1 1 0 0 1 -1 1 h -4 a 1 1 0 0 1 -1 -1 l 0 -14 M 4 20 h 14")

        ''' <summary>« stop » : player-stop (Tabler).</summary>
        Public Shared ReadOnly Property [Stop] As Geometry = Dessin("M 5 7 a 2 2 0 0 1 2 -2 h 10 a 2 2 0 0 1 2 2 v 10 a 2 2 0 0 1 -2 2 h -10 a 2 2 0 0 1 -2 -2 l 0 -10")

        ''' <summary>« structure » : hierarchy-2 (Tabler).</summary>
        Public Shared ReadOnly Property [Structure] As Geometry = Dessin("M 10 3 h 4 v 4 h -4 l 0 -4 M 3 17 h 4 v 4 h -4 l 0 -4 M 17 17 h 4 v 4 h -4 l 0 -4 M 7 17 l 5 -4 l 5 4 M 12 7 l 0 6")

        ''' <summary>« success » : circle-check (Tabler).</summary>
        Public Shared ReadOnly Property Success As Geometry = Dessin("M 3 12 a 9 9 0 1 0 18 0 a 9 9 0 1 0 -18 0 M 9 12 l 2 2 l 4 -4")

        ''' <summary>« table-settings » : table-options (Tabler).</summary>
        Public Shared ReadOnly Property TableSettings As Geometry = Dessin("M 12 21 h -7 a 2 2 0 0 1 -2 -2 v -14 a 2 2 0 0 1 2 -2 h 14 a 2 2 0 0 1 2 2 v 7 M 3 10 h 18 M 10 3 v 18 M 17.001 19 a 2 2 0 1 0 4 0 a 2 2 0 1 0 -4 0 M 19.001 15.5 v 1.5 M 19.001 21 v 1.5 M 22.032 17.25 l -1.299 0.75 M 17.27 20 l -1.3 0.75 M 15.97 17.25 l 1.3 0.75 M 20.733 20 l 1.3 0.75")

        ''' <summary>« unknown » : help-circle (Tabler).</summary>
        Public Shared ReadOnly Property Unknown As Geometry = Dessin("M 3 12 a 9 9 0 1 0 18 0 a 9 9 0 0 0 -18 0 M 12 16 v 0.01 M 12 13 a 2 2 0 0 0 0.914 -3.782 a 1.98 1.98 0 0 0 -2.414 0.483")

        ''' <summary>« unlocked » : lock-open (Tabler).</summary>
        Public Shared ReadOnly Property Unlocked As Geometry = Dessin("M 5 13 a 2 2 0 0 1 2 -2 h 10 a 2 2 0 0 1 2 2 v 6 a 2 2 0 0 1 -2 2 h -10 a 2 2 0 0 1 -2 -2 l 0 -6 M 11 16 a 1 1 0 1 0 2 0 a 1 1 0 1 0 -2 0 M 8 11 v -5 a 4 4 0 0 1 8 0")

        ''' <summary>« unpin » : pinned-off (Tabler).</summary>
        Public Shared ReadOnly Property Unpin As Geometry = Dessin("M 3 3 l 18 18 M 15 4.5 l -3.249 3.249 m -2.57 1.433 l -2.181 0.818 l -1.5 1.5 l 7 7 l 1.5 -1.5 l 0.82 -2.186 m 1.43 -2.563 l 3.25 -3.251 M 9 15 l -4.5 4.5 M 14.5 4 l 5.5 5.5")

        ''' <summary>« upload » : upload (Tabler).</summary>
        Public Shared ReadOnly Property Upload As Geometry = Dessin("M 4 17 v 2 a 2 2 0 0 0 2 2 h 12 a 2 2 0 0 0 2 -2 v -2 M 7 9 l 5 -5 l 5 5 M 12 4 l 0 12")

        ''' <summary>« upload-file » : file-upload (Tabler).</summary>
        Public Shared ReadOnly Property UploadFile As Geometry = Dessin("M 14 3 v 4 a 1 1 0 0 0 1 1 h 4 M 17 21 h -10 a 2 2 0 0 1 -2 -2 v -14 a 2 2 0 0 1 2 -2 h 7 l 5 5 v 11 a 2 2 0 0 1 -2 2 M 12 11 v 6 M 9.5 13.5 l 2.5 -2.5 l 2.5 2.5")

        ''' <summary>« upload-folder » : folder-up (Tabler).</summary>
        Public Shared ReadOnly Property UploadFolder As Geometry = Dessin("M 12 19 h -7 a 2 2 0 0 1 -2 -2 v -11 a 2 2 0 0 1 2 -2 h 4 l 3 3 h 7 a 2 2 0 0 1 2 2 v 3.5 M 19 22 v -6 M 22 19 l -3 -3 l -3 3")

        ''' <summary>« view-cards » : layout-cards (Tabler).</summary>
        Public Shared ReadOnly Property ViewCards As Geometry = Dessin("M 4 6 a 2 2 0 0 1 2 -2 h 2 a 2 2 0 0 1 2 2 v 12 a 2 2 0 0 1 -2 2 h -2 a 2 2 0 0 1 -2 -2 l 0 -12 M 14 6 a 2 2 0 0 1 2 -2 h 2 a 2 2 0 0 1 2 2 v 6 a 2 2 0 0 1 -2 2 h -2 a 2 2 0 0 1 -2 -2 l 0 -6")

        ''' <summary>« view-list » : layout-list (Tabler).</summary>
        Public Shared ReadOnly Property ViewList As Geometry = Dessin("M 4 6 a 2 2 0 0 1 2 -2 h 12 a 2 2 0 0 1 2 2 v 2 a 2 2 0 0 1 -2 2 h -12 a 2 2 0 0 1 -2 -2 l 0 -2 M 4 16 a 2 2 0 0 1 2 -2 h 12 a 2 2 0 0 1 2 2 v 2 a 2 2 0 0 1 -2 2 h -12 a 2 2 0 0 1 -2 -2 l 0 -2")

        ''' <summary>« view-tiles » : layout-grid (Tabler).</summary>
        Public Shared ReadOnly Property ViewTiles As Geometry = Dessin("M 4 5 a 1 1 0 0 1 1 -1 h 4 a 1 1 0 0 1 1 1 v 4 a 1 1 0 0 1 -1 1 h -4 a 1 1 0 0 1 -1 -1 l 0 -4 M 14 5 a 1 1 0 0 1 1 -1 h 4 a 1 1 0 0 1 1 1 v 4 a 1 1 0 0 1 -1 1 h -4 a 1 1 0 0 1 -1 -1 l 0 -4 M 4 15 a 1 1 0 0 1 1 -1 h 4 a 1 1 0 0 1 1 1 v 4 a 1 1 0 0 1 -1 1 h -4 a 1 1 0 0 1 -1 -1 l 0 -4 M 14 15 a 1 1 0 0 1 1 -1 h 4 a 1 1 0 0 1 1 1 v 4 a 1 1 0 0 1 -1 1 h -4 a 1 1 0 0 1 -1 -1 l 0 -4")

        ''' <summary>« waiting » : hourglass (Tabler).</summary>
        Public Shared ReadOnly Property Waiting As Geometry = Dessin("M 6.5 7 h 11 M 6.5 17 h 11 M 6 20 v -2 a 6 6 0 1 1 12 0 v 2 a 1 1 0 0 1 -1 1 h -10 a 1 1 0 0 1 -1 -1 M 6 4 v 2 a 6 6 0 1 0 12 0 v -2 a 1 1 0 0 0 -1 -1 h -10 a 1 1 0 0 0 -1 1")

        ''' <summary>« warning » : alert-triangle (Tabler).</summary>
        Public Shared ReadOnly Property Warning As Geometry = Dessin("M 12 9 v 4 M 10.363 3.591 l -8.106 13.534 a 1.914 1.914 0 0 0 1.636 2.871 h 16.214 a 1.914 1.914 0 0 0 1.636 -2.87 l -8.106 -13.536 a 1.914 1.914 0 0 0 -3.274 0 M 12 16 h 0.01")

        ''' <summary>« zoom-in » : zoom-in (Tabler).</summary>
        Public Shared ReadOnly Property ZoomIn As Geometry = Dessin("M 3 10 a 7 7 0 1 0 14 0 a 7 7 0 1 0 -14 0 M 7 10 l 6 0 M 10 7 l 0 6 M 21 21 l -6 -6")

        ''' <summary>« zoom-out » : zoom-out (Tabler).</summary>
        Public Shared ReadOnly Property ZoomOut As Geometry = Dessin("M 3 10 a 7 7 0 1 0 14 0 a 7 7 0 1 0 -14 0 M 7 10 l 6 0 M 21 21 l -6 -6")

    End Class

End Namespace
