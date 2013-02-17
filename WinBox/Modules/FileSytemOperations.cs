using System;
using System.Collections.Generic;
using System.Diagnostics;
using DropNet.Exceptions;
using DropNet.Models;
using WinBox.Model;
using RestSharp;

namespace WinBox.Modules
{
    public interface IFileSytemOperations
    {
        void UploadFile(UploadData uploadData);
        void UploadFile(UploadData filedata, Action<MetaData> success, Action<DropboxException> failed);
    }

    public class FileSytemOperations : IFileSytemOperations
    {
        public void UploadFile(UploadData uploadData)
        {
            UploadFile(uploadData, OnUploadSuccess, OnFail);
        }

        public void UploadFile(UploadData filedata, Action<MetaData> success, Action<DropboxException> failed)
        {   
            App.DropboxClient.UploadFileAsync(filedata.Path, filedata.Name, filedata.FileData, success, failed);
        }

        void OnFail(DropboxException obj)
        {
            Debug.WriteLine(obj.Message);
        }

        void OnUploadSuccess(MetaData response)
        {
            Debug.WriteLine(response.Name);
        }
    }

    public class UploadQueue<T>
    {
        public void Enqueue(T data)
        {
            _internalQueue.Enqueue(data);
            OnItemEnqueued();
        }

        public T Dequeue()
        {
            return _internalQueue.Dequeue();
        }

        public int Count
        {
            get { return _internalQueue.Count; }
        }

        public event EventHandler ItemEnqueued;

        void OnItemEnqueued()
        {
            var handler = ItemEnqueued;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        readonly Queue<T> _internalQueue = new Queue<T>();
    }

}