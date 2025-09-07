// tax.js
console.log("tax.js loaded ✅");

(function () {
    const cache = new Map();

    function brandColor(name, fallback) {
        const v = getComputedStyle(document.documentElement).getPropertyValue(name).trim();
        return v || fallback;
    }

    const COL_PRIMARY = brandColor('--color-primary', '#6a8dff');
    const COL_SECONDARY = brandColor('--color-secondary', '#9b6aff');

    function destroy(el) {
        const id = el?.id;
        if (!id) return;
        const chart = cache.get(id);
        if (chart) {
            chart.destroy();
            cache.delete(id);
        }
    }

    function mkPie(el, tax, net) {
        if (!el) return;
        destroy(el);
        cache.set(el.id, new Chart(el, {
            type: 'doughnut',
            data: {
                labels: ['Net (Take-home)', 'Tax'],
                datasets: [{
                    data: [net, tax],
                    backgroundColor: [COL_PRIMARY, COL_SECONDARY]
                }]
            },
            options: {
                cutout: '68%',
                plugins: { legend: { position: 'bottom' } },
                maintainAspectRatio: false,
                responsive: true
            }
        }));
    }

    function mkBar(el, gross, tax, net) {
        if (!el) return;
        destroy(el);
        cache.set(el.id, new Chart(el, {
            type: 'bar',
            data: {
                labels: ['Gross', 'Tax', 'Net'],
                datasets: [{
                    label: 'Amount ($)',
                    data: [gross, tax, net],
                    backgroundColor: [COL_PRIMARY, COL_SECONDARY, '#4caf50']
                }]
            },
            options: {
                plugins: { legend: { display: false } },
                scales: { y: { beginAtZero: true } },
                maintainAspectRatio: false,
                responsive: true
            }
        }));
    }

    // Expose a single API that accepts canvas refs from Blazor
    window.ZvTaxCharts = {
        updateByRef: function (pieCanvas, barCanvas, gross, tax, net) {
            console.log("[TaxCharts] update gross=%s, tax=%s, net=%s", gross, tax, net);
            if (pieCanvas) mkPie(pieCanvas, tax, net);
            if (barCanvas) mkBar(barCanvas, gross, tax, net);
        }
    };
})();
