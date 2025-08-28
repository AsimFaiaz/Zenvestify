document.addEventListener("DOMContentLoaded", () => {
    console.log("dashboard.js loaded ✅");

    window.ZvCharts = (() => {
        const cache = {}; // id -> Chart instance

        function mkSpark(id, data) {
            const el = document.getElementById(id);
            if (!el) return;
            if (cache[id]) { cache[id].destroy(); delete cache[id]; }
            cache[id] = new Chart(el, {
                type: 'line',
                data: {
                    labels: data.map((_, i) => i + 1),
                    datasets: [{ data, tension: .35, pointRadius: 0, borderWidth: 2 }]
                },
                options: {
                    maintainAspectRatio: false,
                    responsive: true,
                    plugins: { legend: { display: false } },
                    scales: { x: { display: false }, y: { display: false } }
                }
            });
        }

        function mkPie(id, values) {
            const el = document.getElementById(id);
            if (!el) return;
            if (cache[id]) { cache[id].destroy(); delete cache[id]; }
            cache[id] = new Chart(el, {
                type: 'doughnut',
                data: {
                    labels: ['Groceries', 'Fuel', 'Utilities', 'Other'],
                    datasets: [{ data: values }]
                },
                options: { cutout: '68%', plugins: { legend: { display: false } } }
            });
        }

        function mkBar(id, income, expenses, labels) {
            const el = document.getElementById(id);
            if (!el) return;
            if (cache[id]) { cache[id].destroy(); delete cache[id]; }
            cache[id] = new Chart(el, {
                type: 'bar',
                data: {
                    labels: labels ?? ['W1', 'W2', 'W3', 'W4', 'W5', 'W6'],
                    datasets: [
                        { label: 'Income', data: income },
                        { label: 'Expenses', data: expenses }
                    ]
                },
                options: {
                    plugins: { legend: { position: 'bottom' } },
                    scales: { y: { beginAtZero: true } }
                }
            });
        }

        // Call this after each Blazor render (safe to call multiple times)
        function initAll() {
            mkSpark('sparkIncome', [8, 10, 9, 12, 11, 14, 15]);
            mkSpark('sparkExpense', [4, 6, 5, 7, 8, 6, 7]);
            mkSpark('sparkTax', [6, 6.2, 6.5, 6.8, 7.1, 7.4, 7.6]);
            mkPie('pieCategories', [40, 20, 25, 15]);
            mkBar('trendIncomeExpense', [2, 2, 2, 2, 2, 2], [0.7, 1.0, 0.8, 1.2, 0.9, 0.95]);
        }

        // ✅ expose public API
        return { initAll };

    })();
});
