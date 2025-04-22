window.addEventListener('load', function () {
    var crs = document.querySelectorAll(".course");

    crs.forEach(s => {
        s.addEventListener('mouseover', function () {
            var img = this.querySelector("img");
            var p = this.querySelector(".preview-label");

            if (img) {
                img.style.backgroundColor = "rgba(255, 0, 0, 0.1)";
                img.style.transform = "translateY(-5px)";
            }
            if (p) {
                p.style.visibility = "visible";
            }
        });

        s.addEventListener('mouseout', function () {
            var img = this.querySelector("img");
            var p = this.querySelector(".preview-label");

            if (img) {
                img.style.transform = "translateY(0)";
                img.style.backgroundColor = "";
            }
            if (p) {
                p.style.visibility = "hidden";
            }
        });

        s.addEventListener('click', function () {
            var id = this.getAttribute("data-model-id");
            window.location.href = `/Courses/GetCoursebyid/${id}`;
        });
    });
});
