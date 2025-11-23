document.addEventListener("DOMContentLoaded", () => {
    const img = document.querySelector(".avatar");

    img.addEventListener("click", () => {
        const clone = img.cloneNode();
        clone.style.maxWidth = "400px";
        clone.style.borderRadius = "12px";

        const overlay = document.createElement("div");
        overlay.style.position = "fixed";
        overlay.style.top = 0;
        overlay.style.left = 0;
        overlay.style.right = 0;
        overlay.style.bottom = 0;
        overlay.style.background = "rgba(0,0,0,0.7)";
        overlay.style.display = "flex";
        overlay.style.alignItems = "center";
        overlay.style.justifyContent = "center";
        overlay.style.cursor = "zoom-out";
        overlay.appendChild(clone);

        document.body.appendChild(overlay);

        overlay.onclick = () => overlay.remove();
    });
});

document.addEventListener("DOMContentLoaded", function() {
    // Xử lý fallback ảnh avatar nếu load thất bại
    const avatars = document.querySelectorAll('.student-profile .avatar');
    avatars.forEach(img => {
        img.onerror = function() {
            this.src = '/assets/images/default-avatar.png';
        };
    });

    // Ví dụ: hover highlight item
    const items = document.querySelectorAll('.info-grid .item');
    items.forEach(item => {
        item.addEventListener('mouseenter', () => {
            item.style.backgroundColor = '#eef5ff';
        });
        item.addEventListener('mouseleave', () => {
            item.style.backgroundColor = '#f9f9f9';
        });
    });
});

