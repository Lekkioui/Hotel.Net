   M S B A M L   `   `   `  ����G  CGestionHotel, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null+clr-namespace:GestionHotelGestionHotel  5      #    GestionHotel.ReservationsPage   5      W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      ,localclr-namespace:GestionHotel     5        N� Title$  ReservationsPage��5       � 	Resources 5
       �� 
TargetType �� Property �� Value �� 
TargetType �� CornerRadius �� Triggers P� Property	 P� Value&   System.Windows.Controls.DataGrid=  7System.Windows.Controls.Primitives.DataGridColumnHeader*  $System.Windows.Controls.DataGridCell��%  '��       '  �    '  �    '  �    �� 5   
   " ��6   .�5      ��  � �� �#2196F36+   �� 5       � ��
 �����6+   �� 5       � ��$ 0q�60   �� 5       � ��$
 15,8q�6(   �� 5       � ��$	 100��6)   �� 5       � ��$ 35��6*   �� 5       � ��$ 5q�6'   �� 5       � �� 5      �� 5      " ��6&   .R�5      �� #��z��6!   #��z��5   !   #��z��5   !   $ 4��5   !   ._�5      �� $��Center=�6/   $��Center=�5   -   5      5      5        5!      P� 5"       �@J�IsMouseOver$
	 True��61   . �5#      ��  � �� �#1976D263   5$      P� 5%       � ��$
	 True��6/   . �5&      ��  � �� �#0D47A163   5'      5)      �� 5,   
   "  6   .�5-      ��  � ��
 �����6+   �� 5.       � �� �#DDDDDD6,   �� 5/       � ��$ 1q�60   �� 50       �@ RowBackground
 �����6.   �� 51        �@ AlternatingRowBackground �#F5F5F569   �� 52       �@ 	RowHeight$ 35Q�6*   �� 53        �@ HorizontalGridLinesBrush �#E0E0E069   �� 54       �@ VerticalGridLinesBrush �#E0E0E067   �� 55       �@ HeadersVisibility$ Column=�62   56      �� 59   
   "  6   .�5:      ��  � �� �#1565C06+   �� 5;       � ��
 �����6+   �� 5<       � ��$ SemiBold"�6+   �� 5=       � ��$
 10,8q�6(   �� 5>       � ��$ 0,0,1,0q�60   �� 5?       � �� �#1976D26,   5@      �� 5C   
   "  6   .�5D      ��  � ��$
 10,5q�6(   �� 5E       � �� 5F      �� 5G      "  6&   .R�5H      �� #��z��6!   #��z��5I   !   #��z��5J   !   ._�5K      �� $��Center=�6/   5L      5M      5O      5P      5Q      .��5S      � ���#F5F5F5
 � RowDefinitions
 5T   
   �� 5U      $
��Auto �6   �� 5V      $��* �6   �� 5W      $
��Auto �6   .+�5[   
   �� $��0��6   ���#1565C0 �� Padding$ 20,15q�5]      ._�5^      �� $��Gestion des Réservations��6   
�������$��22(�5`      $��SemiBold"�5a      5b      �� 5e   
   $��1��6   
�������$��20q�5g      $ 8��5h         MaterialDropShadowEffect J� Effect# �   5i      ._�5j        -   5        $��ReservationsDataGrid��5j      $��10q�5k         AutoGenerateColumns$ False��5l         
IsReadOnly$
 True��5m         SelectionMode$ Single=�5n         SelectionUnit$ FullRow=�5o         CanUserResizeRows$ False��5p         Columns 5q      0  *System.Windows.Controls.DataGridTextColumn  5r      ,  &System.Windows.Controls.DataGridColumn   Header$ ID��6)      Width5  /System.Windows.Controls.DataGridLengthConverter$
 Auto 5t   '      MinWidth$ 50Q�5u   '   1  +System.Windows.Controls.DataGridBoundColumn   Binding 5s   '   �� *ReservationID+5u   '     5v      $ Client��6)   $ * 5x   '   $	 150Q�5y   '    5w   '   �� *	ClientNom+5y   '     5z      $ Chambre��6)   $
 Auto 5|   '   $ 80Q�5}   '    5{   '   �� *ChambreNumero+5}   '     5~      $ Arrivée��6)   $
 Auto 5�   '   $	 100Q�5�   '    5   '   �� *DateArrivee+ �� StringFormat$ 
dd/MM/yyyy��5�   '     5�      $ Départ��6)   $
 Auto 5�   '   $	 100Q�5�   '    5�   '   �� *
DateDepart+$ 
dd/MM/yyyy��5�   '   5�      5�      �� 5�   
   $��2��6   
�������$��20q�5�      $ 8��5�      # �   5�      ._�5�      ��  �� Orientation$ 
Horizontal=�6   $��15q�5�      $
��Left=�5�      .+�5�      �� -   5        $��	AddButton��5�      $��Ajouter��5�      5�      �� 5�      -   5        $��ModifyButton��5�      $��Modifier��5�      ���#4CAF505�      �� 5�      -   5        $��DeleteButton��5�      $��	Supprimer��5�      ���#F443365�      5�      5�      5�      5�      