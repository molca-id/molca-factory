var OpenURLPlugin = {
    openURL: function(url) {
        window.open(url, '_blank');
    }
};

mergeInto(LibraryManager.library, OpenURLPlugin);