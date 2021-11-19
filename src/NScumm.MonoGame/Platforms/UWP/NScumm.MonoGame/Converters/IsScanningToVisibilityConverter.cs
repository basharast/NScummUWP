﻿using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace NScumm.MonoGame.Converters
{
    class IsScanningToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var isScanning = (bool)value;
            return isScanning ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
