// ScholarHub - sidebar behaviour, delete confirmation, loading buttons
(function () {
  var body = document.body;
  var toggle = document.getElementById('sidebarToggle');
  var backdrop = document.getElementById('backdrop');
  var isMobile = function () { return window.matchMedia('(max-width: 900px)').matches; };

  // Restore collapsed state on desktop
  try { if (!isMobile() && localStorage.getItem('sh-collapsed') === '1') body.classList.add('sidebar-collapsed'); } catch (e) {}

  toggle && toggle.addEventListener('click', function () {
    if (isMobile()) { body.classList.toggle('drawer-open'); }
    else {
      var c = body.classList.toggle('sidebar-collapsed');
      try { localStorage.setItem('sh-collapsed', c ? '1' : '0'); } catch (e) {}
    }
  });
  backdrop && backdrop.addEventListener('click', function () { body.classList.remove('drawer-open'); });
  document.querySelectorAll('.nav-link').forEach(function (a) {
    a.addEventListener('click', function () { body.classList.remove('drawer-open'); });
  });
  document.addEventListener('keydown', function (e) { if (e.key === 'Escape') body.classList.remove('drawer-open'); });

  // Confirmation before destructive POST forms
  document.querySelectorAll('form[data-confirm]').forEach(function (f) {
    f.addEventListener('submit', function (e) { if (!confirm(f.getAttribute('data-confirm'))) e.preventDefault(); });
  });

  // Loading state on submit buttons
  document.querySelectorAll('form.card').forEach(function (f) {
    f.addEventListener('submit', function () {
      var b = f.querySelector('button[type=submit]');
      if (b) setTimeout(function () { b.disabled = true; b.classList.add('loading'); b.textContent = 'Saving...'; }, 0);
    });
  });

  // Auto-hide success alerts
  setTimeout(function () { document.querySelectorAll('.alert-success').forEach(function (a) { a.style.display = 'none'; }); }, 5000);
})();

// ScholarHub - attendance "mark all" buttons and print buttons
(function () {
  document.querySelectorAll('[data-mark-all]').forEach(function (btn) {
    btn.addEventListener('click', function () {
      var value = btn.getAttribute('data-mark-all');
      document.querySelectorAll('input[type=radio][value="' + value + '"]').forEach(function (r) { r.checked = true; });
    });
  });
  document.querySelectorAll('[data-print]').forEach(function (btn) {
    btn.addEventListener('click', function () { window.print(); });
  });
  // ScholarHub - show only the courses that belong to the selected department
(function () {
  var dept = document.getElementById('DepartmentId');
  var course = document.getElementById('CourseId');
  if (!dept || !course || !course.querySelector('option[data-dept]')) return;

  function filterCourses() {
    var d = dept.value;
    Array.prototype.forEach.call(course.options, function (o) {
      var od = o.getAttribute('data-dept');
      var show = !od || (d !== '0' && od === d);
      o.hidden = !show;
      o.disabled = !show;
    });
    if (course.options[course.selectedIndex] && course.options[course.selectedIndex].disabled) course.value = '0';
  }
  dept.addEventListener('change', filterCourses);
  filterCourses();
})();
})();
