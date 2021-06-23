﻿// ****************************************************************************
// <copyright file="IEventArgsConverter.cs" company="GalaSoft Laurent Bugnion">
// Copyright © GalaSoft Laurent Bugnion 2009-2016
// </copyright>
// ****************************************************************************
// <author>Laurent Bugnion</author>
// <email>laurent@galasoft.ch</email>
// <date>18.05.2013</date>
// <project>GalaSoft.MvvmLight.Extras</project>
// <web>http://www.mvvmlight.net</web>
// <license>
// See license.txt in this solution or http://www.galasoft.ch/license_MIT.txt
// </license>
// ****************************************************************************

namespace boilersGraphics.Helpers
{
    /// <summary>
    /// The definition of the converter used to convert an EventArgs
    /// in the <see cref="EventToCommand"/> class, if the
    /// <see cref="EventToCommand.PassEventArgsToCommand"/> property is true.
    /// Set an instance of this class to the <see cref="EventToCommand.EventArgsConverter"/>
    /// property of the EventToCommand instance.
    /// </summary>
    ////[ClassInfo(typeof(EventToCommand))]
    public interface IEventArgsConverter
    {
        /// <summary>
        /// The method used to convert the EventArgs instance.
        /// </summary>
        /// <param name="value">An instance of EventArgs passed by the
        /// event that the EventToCommand instance is handling.</param>
        /// <param name="parameter">An optional parameter used for the conversion. Use
        /// the <see cref="EventToCommand.EventArgsConverterParameter"/> property
        /// to set this value. This may be null.</param>
        /// <returns>The converted value.</returns>
        object Convert(object value, object parameter);
    }
}
