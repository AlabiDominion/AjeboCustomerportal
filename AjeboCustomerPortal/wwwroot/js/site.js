window.addEventListener("load", function () {
    setTimeout(() => {
        document.getElementById("loader").style.display = "none";
        document.getElementById("content").style.display = "block";
    }, 3000); 
});

let HamburgerFirst = document.getElementById('HamburgerFirst')
let HamburgerSecond = document.getElementById('HamburgerSecond')
let topbarDropPanel = document.getElementById('topbarDropPanel')

HamburgerFirst.addEventListener('click', () => {
    HamburgerFirst.classList.toggle('DisplayHamburger')
    HamburgerSecond.classList.toggle('DisplayHamburger')
    topbarDropPanel.style.display = 'flex'
})

HamburgerSecond.addEventListener('click', () => {
    HamburgerFirst.classList.toggle('DisplayHamburger')
    HamburgerSecond.classList.toggle('DisplayHamburger')
    topbarDropPanel.style.display = 'none'
})