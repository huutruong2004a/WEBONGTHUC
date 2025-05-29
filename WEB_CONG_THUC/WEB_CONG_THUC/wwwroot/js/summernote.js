$(document).ready(function () {
    $('.summernote').summernote({
        height: 300,
        fontSizes: ['8', '10', '12', '14', '16', '18', '24', '36', '48', '64', '82', '150'],
        fontNames: ['Arial', 'Arial Black', 'Comic Sans MS', 'Courier New', 'Georgia', 'Impact', 'Tahoma', 'Times New Roman', 'Trebuchet MS', 'Verdana'],
        fontNamesIgnoreCheck: ['Arial', 'Arial Black', 'Comic Sans MS', 'Courier New', 'Georgia', 'Impact', 'Tahoma', 'Times New Roman', 'Trebuchet MS', 'Verdana'],
        toolbar: [
            ['style', ['style']],
            ['font', ['fontname', 'fontsize', 'bold', 'italic', 'underline', 'clear']],
            ['color', ['color']],
            ['para', ['ul', 'ol', 'paragraph']],
            ['height', ['height']],
            ['insert', ['picture', 'link', 'video']],
            ['view', ['fullscreen', 'codeview', 'help']]
        ],
        callbacks: {
            onImageUpload: function (files) {
                for (var i = 0; i < files.length; i++) {
                    uploadImage(files[i]);
                }
            }
        }
    });

    function uploadImage(file) {
        var data = new FormData();
        data.append("image", file);

        $.ajax({
            url: '/Blog/UploadImage',
            type: "POST",
            data: data,
            contentType: false,
            processData: false,
            success: function (url) {
                $('.summernote').summernote('insertImage', url);
            },
            error: function () {
                alert("Lỗi khi upload ảnh.");
            }
        });
    }
});
