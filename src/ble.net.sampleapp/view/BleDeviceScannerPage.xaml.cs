using System;
using ble.net.sampleapp.viewmodel;
using Xamarin.Forms;
using System.ComponentModel;
using System.Collections.Generic;

namespace ble.net.sampleapp.view
{
   public partial class BleDeviceScannerPage // 部分类，另外一个部分就是对应的xmal，编译时编译器回自动将前端代码翻译成C# 
   {
      public BleDeviceScannerPage( BleDeviceScannerViewModel vm )
      {
         //这个函数在哪儿定义呢？同样是在对应的xaml文件中编译自动翻译生成的，自动翻译的文件在debug文件夹中，带有.g的后缀。
         //这个函数会自动调用.NETstandard库中的LoadFromXaml方法，这个函数功能十分强大，用于整合所有前后端资源用于生成界面。
         InitializeComponent();
         BindingContext = vm;
      }

      private void ListView_OnItemSelected( Object sender, SelectedItemChangedEventArgs e )
      {
         if(e.SelectedItem != null)
         {
            //((BlePeripheralViewModel)e.SelectedItem).IsExpanded = !((BlePeripheralViewModel)e.SelectedItem).IsExpanded;
            ((ListView)sender).SelectedItem = null;
            
         }
      }

      private void ListView_OnItemTapped( Object sender, ItemTappedEventArgs e )
      {
      }


         private void Switch_OnToggled( Object sender, ToggledEventArgs e )
      {
         var vm = BindingContext as BleDeviceScannerViewModel;
         if(vm == null)
         {
            return;
         }
         if(e.Value)
         {
            if(vm.EnableAdapterCommand.CanExecute( null ))
            {
               vm.EnableAdapterCommand.Execute( null );
            }
         }
         else if(vm.DisableAdapterCommand.CanExecute( null ))
         {
            vm.DisableAdapterCommand.Execute( null );
         }
      }
   }
 }
