console.log("income.js loaded ✅");

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

    function mkTrend(el, primary, others) {
        if (!el) return;
        destroy(el);
        cache.set(el.id, new Chart(el, {
            type: 'line',
            data: {
                labels: primary?.months ?? [],
                datasets: [
                    {
                        label: 'Primary',
                        data: primary?.values ?? [],
                        borderColor: COL_PRIMARY,
                        fill: false
                    },
                    {
                        label: 'Other',
                        data: others?.values ?? [],
                        borderColor: COL_SECONDARY,
                        fill: false
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false
            }
        }));
    }

    function mkPie(el, sources) {
        if (!el) return;
        destroy(el);

        const labels = sources.map(s => s.source);
        const values = sources.map(s => s.amount);

        // generate dynamic shades
        const colors = labels.map((_, i) => {
            const base = i % 2 === 0 ? COL_PRIMARY : COL_SECONDARY;
            return base + Math.floor(80 + (i * 20) % 120).toString(16);
        });

        cache.set(el.id, new Chart(el, {
            type: 'doughnut',
            data: {
                labels,
                datasets: [{
                    data: values,
                    backgroundColor: colors
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

    // ✅ Expose to Blazor
    window.ZvIncomeCharts = {
        init: function (primary, others) {
            console.log("[ZvIncomeCharts.init]", primary, others);

            const trendEl = document.getElementById("incomeTrend");
            const pieEl = document.getElementById("incomePie");

            if (trendEl) mkTrend(trendEl, primary, others);
            if (pieEl) mkPie(pieEl, others);
        }
    };
})();
