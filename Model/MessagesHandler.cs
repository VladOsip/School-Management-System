﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace EasySchool.Model
{
    /// <summary>
    /// All possible recipients for messages
    /// </summary>
    public enum MessageRecipientsTypes
    {
        Person,
        Class,
        Parent,
        Students,
        Teachers,
        Management,
        Everyone,
    }

    /// <summary>
    /// Message creation handler
    /// </summary>
    public static class MessagesHandler
    {
        #region Fields
        private static SchoolEntities _schoolEntityDB;
        #endregion

        #region Constructors
        static MessagesHandler()
        {
            _schoolEntityDB = new SchoolEntities();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Creates a new message and saves it
        /// </summary>
        /// <param name="title">The title of the message</param>
        /// <param name="text">The actual content of the message</param>
        /// <param name="recipientType">The type of recipients for this message</param>
        /// <param name="senderID">Person ID of the sender. If null, it is an automatic message</param>
        /// <param name="recipientID">The ID of the recipient</param>
        static public void CreateMessage(string title, string text, MessageRecipientsTypes recipientType, int? senderID = null, int? recipientID = null)
        {
            // If this recipientType requires an specific person/class ID, make sure it is defined and not null
            if (recipientType == MessageRecipientsTypes.Person ||
                recipientType == MessageRecipientsTypes.Class ||
                recipientType == MessageRecipientsTypes.Parent)
            {
                if (!recipientID.HasValue)
                {
                    throw new ArgumentNullException("recipientID", "recipientID must be defined for direct messages");
                }
            }

            // Check recipient type and create a suitable message
            switch (recipientType)
            {
                case MessageRecipientsTypes.Class:
                    CreateMessageToClass(title, text, recipientID.Value, senderID);
                    break;
                case MessageRecipientsTypes.Person:
                case MessageRecipientsTypes.Parent:
                    CreateMessageToPerson(title, text, recipientID.Value, senderID);
                    break;
                case MessageRecipientsTypes.Everyone:
                    CreateMessageToEveryone(title, text, senderID);
                    break;
                case MessageRecipientsTypes.Management:
                    CreateMessageToAllManagement(title, text, senderID);
                    break;
                case MessageRecipientsTypes.Teachers:
                    CreateMessageToTeachers(title, text, senderID);
                    break;
                case MessageRecipientsTypes.Students:
                    CreateMessageToAllStudents(title, text, senderID);
                    break;
            }
        }

        /// <summary>
        /// An helper method that creates a simple template of a message 
        /// </summary>
        /// <param name="title">The title of the message</param>
        /// <param name="text">The actual message content</param>
        /// <param name="senderID">The ID of the sender. If this is null then this is an automatic message</param>
        /// <returns></returns>
        static private Message CreateBaseMessage(string title, string text, int? senderID = null)
        {
            Message newMessage = new Message();

            newMessage.senderID = senderID;
            newMessage.recipientID = null;
            newMessage.recipientClassID = null;

            newMessage.title = title;
            newMessage.data = text;
            newMessage.date = DateTime.Now;

            newMessage.forAllManagement = false;
            newMessage.forAllTeachers = false;
            newMessage.forAllStudents = false;
            newMessage.forEveryone = false;

            return newMessage;
        }

        /// <summary>
        /// Save a message to the DB
        /// </summary>
        static private void SaveMessage(Message newMessage)
        {
            _schoolEntityDB.Messages.Add(newMessage);
            _schoolEntityDB.SaveChanges();
        }
        /// <summary>
        /// Creates a new message to a specific person and saves it
        /// </summary>
        /// <param name="title">The title of the message</param>
        /// <param name="text">The actual content of the message</param>
        /// <param name="recipientID">Person ID of the recipient</param>
        /// <param name="sender">Person ID of the sender. If null, it is an automatic message</param>
        static private void CreateMessageToPerson(string title, string text, int recipientID, int? sender=null)
        {
            Message newMessage = CreateBaseMessage(title, text, sender);
            newMessage.recipientID = recipientID;
            SaveMessage(newMessage);
        }

        /// <summary>
        /// Generate and save a Message for management (pinciple/secrateries)
        /// </summary>
        /// <param name="title">message title</param>
        /// <param name="text">message content</param>
        /// <param name="senderID">Sender Person key. null means auto message</param>
        static private void CreateMessageToAllManagement(string title, string text, int? senderID = null)
        {
            Message newMessage = CreateBaseMessage(title, text, senderID);
            newMessage.forAllManagement = true;
            SaveMessage(newMessage);
        }

        /// <summary>
        /// Generate and save a Message for a teacher
        /// </summary>
        /// <param name="title">message title</param>
        /// <param name="text">message content</param>
        /// <param name="senderID">Sender Person key. null means auto message</param>
        static private void CreateMessageToTeachers(string title, string text, int? senderID = null)
        {
            Message newMessage = CreateBaseMessage(title, text, senderID);
            newMessage.forAllTeachers = true;
            SaveMessage(newMessage);
        }

        /// <summary>
        /// Generate and save a Message for a student
        /// </summary>
        /// <param name="title">message title</param>
        /// <param name="text">message content</param>
        /// <param name="senderID">Sender Person key. null means auto message</param>
        static private void CreateMessageToAllStudents(string title, string text, int? senderID = null)
        {
            Message newMessage = CreateBaseMessage(title, text, senderID);
            newMessage.forAllStudents = true;
            SaveMessage(newMessage);
        }

        /// <summary>
        /// Generate and save a Message for everyone
        /// </summary>
        /// <param name="title">message title</param>
        /// <param name="text">message content</param>
        /// <param name="senderID">Sender Person key. null means auto message</param>
        static private void CreateMessageToEveryone(string title, string text, int? senderID = null)
        {
            Message newMessage = CreateBaseMessage(title, text, senderID);
            newMessage.forEveryone = true;
            SaveMessage(newMessage);
        }

        /// <summary>
        /// Generate and save a Message for a class
        /// </summary>
        /// <param name="title">message title</param>
        /// <param name="text">message content</param>
        /// <param name="recipientClassID">recipient Class key</param>
        /// <param name="senderID">Sender Person key. null means auto message</param>
        static private void CreateMessageToClass(string title, string text, int recipientClassID, int? senderID = null)
        {
            Message newMessage = CreateBaseMessage(title, text, senderID);
            newMessage.recipientClassID = recipientClassID;
            SaveMessage(newMessage);
        }
        #endregion
    }
}
