   M S B A M L   `   `   `  ����A  =Client, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullclr-namespace:ClientClient  5   	       Client.MainWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   &localclr-namespace:Client     5   	     � Title$  Client��5   	   $	��450��6   $	��810��6%   .��5	      �  � RowDefinitions 5
   
   �� 5      $��* �6   �� 5      $
��Auto �6   .+�5   
   �� -   5        $��	outputBox��5      $��0��5      $��10q�5       �� 
IsReadOnly . " �� VerticalScrollBarVisibility$
 Auto=�5       �� TextWrapping$
 Wrap=�5       �� AcceptsReturn . 5      �� 5   
   $��1��6    �� Orientation$ 
Horizontal=�6"   $��10q�6;   .+�5      �� -   5        $��inputBox��5      $	��690��5      $��30��5      $��0,0,10,0q�5      $��Center=�5      �� 5      -   5         ��Name$ btnSend��5      $��73��5      $��30��5       $��Отправить��5!      $��Normal$�5#      
������5$      5%      5&      5'      