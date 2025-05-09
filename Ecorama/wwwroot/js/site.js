// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.



// Header Scroll Effect
const header = document.querySelector('header');
const navbar = document.querySelector('.navbar');
const hamburger = document.querySelector('.hamburger');
const navLinks = document.querySelector('.nav-links');

window.addEventListener('scroll', () => {
    if (window.scrollY > 100) {
        header.classList.add('scrolled');
    } else {
        header.classList.remove('scrolled');
    }
});

// Mobile Menu
hamburger.addEventListener('click', () => {
    navbar.classList.toggle('active');
    navLinks.classList.toggle('active');
});

// Image Slider
const slider = document.querySelector('.slides');
const dots = document.querySelectorAll('.dot');
let slideIndex = 0;
const totalSlides = document.querySelectorAll('.slide').length;

function showSlide(index) {
    slideIndex = index;
    slider.style.transform = `translateX(${-slideIndex * 25}%)`;
    dots.forEach(dot => dot.classList.remove('active'));
    dots[slideIndex].classList.add('active');
}

dots.forEach((dot, index) => {
    dot.addEventListener('click', () => {
        showSlide(index);
    });
});

function autoSlide() {
    slideIndex = (slideIndex + 1) % totalSlides;
    showSlide(slideIndex);
}

setInterval(autoSlide, 5000);

// Testimonial Slider
const testimonials = document.querySelectorAll('.testimonial');
const testimonialWrapper = document.querySelector('.testimonials-wrapper');
const prevBtn = document.querySelector('.prev-testimonial');
const nextBtn = document.querySelector('.next-testimonial');
let testimonialIndex = 0;

function showTestimonial(index) {
    testimonialIndex = index;
    testimonials.forEach(testimonial => testimonial.classList.remove('active'));
    testimonials[testimonialIndex].classList.add('active');
    testimonialWrapper.style.transform = `translateX(${-testimonialIndex * 100}%)`;
}

prevBtn.addEventListener('click', () => {
    testimonialIndex = (testimonialIndex - 1 + testimonials.length) % testimonials.length;
    showTestimonial(testimonialIndex);
});

nextBtn.addEventListener('click', () => {
    testimonialIndex = (testimonialIndex + 1) % testimonials.length;
    showTestimonial(testimonialIndex);
});

// Stats Counter Animation
const stats = document.querySelectorAll('.stat-number');
const statsSection = document.querySelector('.stats');

function startCounter() {
    stats.forEach(stat => {
        const target = parseInt(stat.getAttribute('data-count'));
        const count = +stat.innerText;
        const increment = target / 50;

        if (count < target) {
            stat.innerText = Math.ceil(count + increment);
            setTimeout(() => startCounter(), 40);
        } else {
            stat.innerText = target;
        }
    });
}

// Intersection Observer for Animation
const observerOptions = {
    threshold: 0.1
};

// Observe stats section
const statsObserver = new IntersectionObserver(function (entries, observer) {
    entries.forEach(entry => {
        if (entry.isIntersecting) {
            startCounter();
            observer.unobserve(entry.target);
        }
    });
}, observerOptions);

statsObserver.observe(statsSection);

// Observe service cards
const serviceCards = document.querySelectorAll('.service-card');
const serviceObserver = new IntersectionObserver(function (entries, observer) {
    entries.forEach((entry, index) => {
        if (entry.isIntersecting) {
            setTimeout(() => {
                entry.target.style.opacity = 1;
                entry.target.style.transform = 'translateY(0)';
            }, index * 200);
            observer.unobserve(entry.target);
        }
    });
}, observerOptions);

serviceCards.forEach(card => {
    serviceObserver.observe(card);
});

// Observe workshop cards
const workshopCards = document.querySelectorAll('.workshop-card');
const workshopObserver = new IntersectionObserver(function (entries, observer) {
    entries.forEach((entry, index) => {
        if (entry.isIntersecting) {
            setTimeout(() => {
                entry.target.style.opacity = 1;
                entry.target.style.transform = 'translateY(0)';
            }, index * 200);
            observer.unobserve(entry.target);
        }
    });
}, observerOptions);

workshopCards.forEach(card => {
    workshopObserver.observe(card);
});

// Observe stat items
const statItems = document.querySelectorAll('.stat-item');
const statObserver = new IntersectionObserver(function (entries, observer) {
    entries.forEach((entry, index) => {
        if (entry.isIntersecting) {
            setTimeout(() => {
                entry.target.style.opacity = 1;
                entry.target.style.transform = 'translateY(0)';
            }, index * 200);
            observer.unobserve(entry.target);
        }
    });
}, observerOptions);

statItems.forEach(item => {
    statObserver.observe(item);
});
