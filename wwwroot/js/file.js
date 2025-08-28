// document.getElementById("fileInput").addEventListener("change", function () {
//     const file = this.files[0];
//     if (file) {
//         document.getElementById("fileName").textContent = file.name;
//         document.getElementById("fileInfo").style.display = "inline-flex";

//         let formData = new FormData(document.getElementById("uploadForm"));
//         fetch('/Chat/AddFile', {
//             method: 'POST',
//             body: formData
//         }).then(res => {
//             if (res.ok) {
//                 console.log("File uploaded");
//             } else {
//                 console.log("File error");
//             }
//         });
//     }
// });



