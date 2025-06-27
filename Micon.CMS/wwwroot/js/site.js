// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// move active class on menu 
document.addEventListener("DOMContentLoaded", function () {
    const listItems = document.querySelectorAll(".aside-menu li");

    listItems.forEach(item => {
        item.addEventListener("click", function (e) {
            e.preventDefault(); // aタグなどのデフォルト動作を無効化

            // クリックされた要素の中で最も近いli要素を取得
            const clickedLi = e.target.closest("li");

            if (!clickedLi) return;

            // 現在のactiveを削除
            document.querySelector(".aside-menu .active")?.classList.remove("active");

            // クリックされた要素にactiveを追加
            clickedLi.classList.add("active");
        });
    });
});