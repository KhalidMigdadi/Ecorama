document.addEventListener('DOMContentLoaded', function () {
    const imageInput = document.getElementById('partnerImage');
    const imagePreview = document.getElementById('imagePreview');
    const uploadPlaceholder = document.getElementById('uploadPlaceholder');
    const imagePreviewContainer = document.getElementById('imagePreviewContainer');
    const resetBtn = document.getElementById("resetBtn");


    imageInput.addEventListener('change', function () {
        const file = this.files[0];
        if (file && file.type.match('image.*')) {
            const reader = new FileReader();
            reader.onload = function (e) {
                imagePreview.src = e.target.result;
                imagePreview.style.display = 'block';
                uploadPlaceholder.style.display = 'none';
            };
            reader.readAsDataURL(file);
        }
    });


    imagePreviewContainer.addEventListener('dragover', function (e) {
        e.preventDefault();
        this.style.borderColor = '#4e73df';
        this.style.backgroundColor = 'rgba(78, 115, 223, 0.1)';
    });

    imagePreviewContainer.addEventListener('dragleave', function (e) {
        e.preventDefault();
        this.style.borderColor = '#e3e6f0';
        this.style.backgroundColor = '#f8f9fc';
    });

    imagePreviewContainer.addEventListener('drop', function (e) {
        e.preventDefault();
        this.style.borderColor = '#e3e6f0';
        this.style.backgroundColor = '#f8f9fc';

        const file = e.dataTransfer.files[0];
        if (file && file.type.match('image.*')) {
            imageInput.files = e.dataTransfer.files;

            const reader = new FileReader();
            reader.onload = function (e) {
                imagePreview.src = e.target.result;
                imagePreview.style.display = 'block';
                uploadPlaceholder.style.display = 'none';
            };
            reader.readAsDataURL(file);
        }
    });


    imagePreviewContainer.addEventListener('click', function () {
        imageInput.click();
    });


    resetBtn.addEventListener("click", function () {
        document.getElementById("addPartnerForm").reset();

        imagePreview.src = "";
        imagePreview.style.display = 'none';
        uploadPlaceholder.style.display = 'block';

        imagePreviewContainer.style.borderColor = '#e3e6f0';
        imagePreviewContainer.style.backgroundColor = '#f8f9fc';
    });
});
