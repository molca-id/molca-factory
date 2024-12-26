mergeInto(LibraryManager.library, {

  IsMobile: function () {
    var ua = window.navigator.userAgent.toLowerCase();
    var mobilePattern = /android|iphone|ipad|ipod/i;

    return ua.search(mobilePattern) !== -1 || (ua.indexOf("macintosh") !== -1 && "ontouchend" in document);
  },

  PlayVideo: function (str) {
    var video = document.getElementById("video");
    var source = document.getElementById('video-source');

    source.setAttribute('src', UTF8ToString(str));
    video.load();

    video.style.display = "block";
    video.addEventListener('ended', function() {
      console.log("Video has finished!");
      video.style.display = "none";
    });
  },

});