document.addEventListener('DOMContentLoaded', () => {
  const navToggle = document.querySelector('.nav-toggle');
  const navLinks = document.querySelector('.nav-links');

  if (navToggle && navLinks) {
    navToggle.addEventListener('click', () => {
      navLinks.classList.toggle('active');
      navToggle.textContent = navLinks.classList.contains('active') ? 'Close' : 'Menu';
    });
  }

  const currentPath = window.location.pathname.replace(/\/$/, '').toLowerCase();
  document.querySelectorAll('.nav-links a').forEach(link => {
    const linkPath = new URL(link.href, window.location.origin).pathname.replace(/\/$/, '').toLowerCase();
    if (linkPath === currentPath) {
      link.classList.add('active');
    }
  });
});
