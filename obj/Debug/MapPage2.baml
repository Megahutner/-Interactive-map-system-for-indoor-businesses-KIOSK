   M S B A M L   `   `   `  ����C  ?UWEKiosk, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null#clr-namespace:UWEKioskUWEKiosk  5   	       UWEKiosk.MapPage2   5      \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   (localclr-namespace:UWEKiosk     5   	     N� Title$  MapPage2��5   	   .��5	      �  � ColumnDefinitions 5
   
   �� 5      -   5         ��Name$
 col1��5      $��0.75* �5      �� 5      -   5        $
 col2��5      $��0.25* �5       � RowDefinitions 5   
   �� 5      -   5         ��Name$
 row1��5      $
��0.9* �5      �� 5      -   5        $
 row2��5      $
��0.1* �5      .+�5      �� -   5        $��mapGrid��5   2   $��0��6   $��0��6"   50      � 52   
   $��0��6   $��1��6   .+�57      �� -   5        $��ListViewBorder��57      
����� �$��3q�6C   $��3 5 3 5q�6W    �� CornerRadius$ 8��6h    �� Padding$ 4q�6y   ._�58      �� -   5        $��zoneObjects��58   .    �� SelectionMode$ Single=�6   $��5 2 5 5q�6A   
������ $	��500��6l   $
��Left=�6x   ��59      �� 5:      .R�5;      �� .+�5<   "   �� $��20(�6B   $��10,0,0,5q�6P   ��6,   �� *Name+6P   5>      5?       �� 
GroupStyle 5B      (  "System.Windows.Controls.GroupStyle  5J      	   ContainerStyle	 5K      �� 5L   "   
 �� 
TargetType"
 ��6(   .�5M   &   ��  �� Property � �� �� Value 5N   *   �� 5O   .   .R�5P   2   9�  9� 
IsExpanded . ��5Q   6   �� 5R   :    �� Orientation$ 
Horizontal=�6E   .+�5S   >   �� $
��Bold"�6^   
�������$��25(�6�   $��Bottom=�6�   ��6H   �� *Name+6�   �� 5T   >   $
��Bold"�6c   
���  ��$��25(�6�   $��Bottom=�6�   $��10,0,0,0q�6�   ��6H   �� *	ItemCount+6�   5U   ;   .��5W   6   �� 5X   3   5Y   /   5[   '   5\   #   5^      5`      5a      5b      5d      5e      