mergeInto(LibraryManager.library, {
    CallJavaScriptFunction: function() {
        startLoginWebGL(); // This function is embeded into the page
    }
});