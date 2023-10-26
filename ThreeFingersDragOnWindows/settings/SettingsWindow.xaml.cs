using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Windows.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ThreeFingersDragOnWindows.utils;
using Windows.ApplicationModel;
using Windows.Foundation.Metadata;
using System.Reflection;
using ThreeFingersDragEngine.utils;

namespace ThreeFingersDragOnWindows.settings;

public sealed partial class SettingsWindow {
    private readonly App App;
    
    /*private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
    {
        var regex = new Regex("[^0-9]+");
        e.Handled = regex.IsMatch(e.Text);
    }*/

    public SettingsWindow(App app, bool openOtherSettings){
        App = app;
        Debug.WriteLine("Starting SettingsWindow...");


        InitializeComponent();
        AppWindow.Resize(new SizeInt32(1000, 600));

        ExtendsContentIntoTitleBar = true; // enable custom titlebar
        SetTitleBar(TitleBar); // set TitleBar element as titlebar
        
        NavigationView.SelectedItem = openOtherSettings ? OtherSettings : Touchpad;
    }


    private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs e){
        if(e.SelectedItem.Equals(Touchpad)){
            sender.Header = "Touchpad";
            ContentFrame.Navigate(typeof(TouchpadSettings));
            
        }if(e.SelectedItem.Equals(ThreeFingersDrag)){
            sender.Header = "Three Fingers Drag";
            ContentFrame.Navigate(typeof(ThreeFingersDragSettings));
            
        } else if(e.SelectedItem.Equals(OtherSettings)){
            sender.Header = "Other Settings";
            ContentFrame.Navigate(typeof(OtherSettings));
        }
    }


    ////////// Close & quit //////////

    private void CloseButton_Click(object sender, RoutedEventArgs e){
        Close();
    }

    private void QuitButton_Click(object sender, RoutedEventArgs e){
        App.Quit();
    }

    private void Window_Closed(object sender, WindowEventArgs e){
        Debug.WriteLine("Hiding SettingsWindow, saving data...");
        App.SettingsData.save();
        
        // Navigate to another page, so the "OnNavigatedFrom()" of OtherSettings gets called (for the timer).
        ContentFrame.Navigate(typeof(TouchpadSettings));
        
        App.OnClosePrefsWindow();
    }

    private int _inputCount;
    private long _lastContact;
    private long _lastEventSpeed;
    public void OnTouchpadContact(TouchpadContact[] contacts){
        _inputCount++;
    
        // Event speed is an average over 20 inputs calls (usually about 200 ms)
        if(_inputCount >= 20){
            _inputCount = 0;
            _lastEventSpeed = (Ctms() - _lastContact) / 20;
            _lastContact = Ctms();
        }
        Page currentPage = ContentFrame.Content as Page;
        if(currentPage is TouchpadSettings touchpadSettings){
            touchpadSettings.UpdateContactsText(string.Join('\n', contacts.Select(c => c.ToString())) + "\nEvent speed: " + _lastEventSpeed + "ms");
        }
    }
    public void OnTouchpadInitialized(){
        Page currentPage = ContentFrame.Content as Page;
        if(currentPage is TouchpadSettings touchpadSettings){
            touchpadSettings.OnTouchpadInitialized();
        }
    }

    private long Ctms(){
        return new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
    }
}