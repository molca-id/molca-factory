using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using NaughtyAttributes;
using UnityEngine.Video;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections;

namespace InteractiveViewer
{
    [Serializable]
    public class PKT_MediaInfo
    {
        public enum Type
        {
            Unknown,
            Image,
            Video,
            Document
        }

        public string name;
        public int id;
        public Type type;
        public bool isAddressable;
        [HideIf(nameof(isAddressable)), AllowNesting]
        public string url;
        [ShowIf(nameof(isAddressable)), AllowNesting]
        public AssetReference asset;

        [HideInInspector]
        public string mime_type;
        [HideInInspector]
        public int version;

        private UnityEngine.Object _loadedAsset; 

        /// <summary>
        /// Runtime generated media info
        /// </summary>
        /// <param name="name"></param>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <param name="url"></param>
        public PKT_MediaInfo (string name, int id, Type type, string url)
        {
            this.name = name;
            this.id = id;
            this.type = type;
            this.isAddressable = false;
            this.url = url;
            this.asset = null;
        }

        public void Init()
        {
            if (mime_type.Contains("image")) type = Type.Image;
            else if (mime_type.Contains("vid")) type = Type.Video;
            else if (mime_type.Contains("pdf")) type = Type.Document;
            else type = Type.Unknown;
        }

        public void Unload()
        {
            if(isAddressable && asset.IsValid())
                asset.ReleaseAsset();
            else if (_loadedAsset != null)
            {
                UnityEngine.Object.Destroy(_loadedAsset);
                _loadedAsset = null;
            }
        }

        public IEnumerator GetTexture(Action<Texture2D> onFinish)
        {
            Debug.Log($"Getting texture from media of type: {type.ToString()}");
            if (type == Type.Image)
            {
                AsyncOperationHandle async;
                if (asset.OperationHandle.IsValid())
                    async = asset.OperationHandle;
                else
                    async = asset.LoadAssetAsync<Texture2D>();

                yield return async;

                onFinish?.Invoke(async.Result as Texture2D);
                yield break;
            }
            else if (type == Type.Video)
            {
                yield return VideoHandler.GetThumbnail(this, (txt) => _loadedAsset = txt);
                onFinish?.Invoke(_loadedAsset as Texture2D);
                yield break;
            }

            Debug.LogWarning("Failed to get texture, invalid media type.");
            onFinish?.Invoke(null);
        }

        public bool PrepareVideo(VideoPlayer vp)
        {
            if (type != Type.Video) return false;
            vp.Stop();

            /*AsyncOperationHandle async;
            if (asset.OperationHandle.IsValid())
                async = asset.OperationHandle;
            else
                async = asset.LoadAssetAsync<VideoClip>();
            yield return async;*/

            if(string.IsNullOrEmpty(url))
                return false;

            vp.Stop();
            vp.source = VideoSource.Url;
            vp.url = url; //async.Result as VideoClip;
            vp.Prepare();
            return true;
        }
    }
}