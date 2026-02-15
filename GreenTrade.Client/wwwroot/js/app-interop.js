window.appInterop = {
    charts: {},

    initializeSneatMenu: function () {
        if (typeof Menu !== 'undefined' && document.querySelector('#layout-menu')) {
             new Menu(document.querySelector('#layout-menu'), {
                orientation: 'vertical',
                closeChildren: false
            });
        }
        
        const menuInnerContainer = document.querySelector('.menu-inner');
        if (menuInnerContainer && typeof PerfectScrollbar !== 'undefined') {
             new PerfectScrollbar(menuInnerContainer, {
                wheelPropagation: false,
                suppressScrollX: true
            });
        }
    },

    toggleMobileMenu: function () {
        // Toggle class on html element for mobile menu
        document.documentElement.classList.toggle('layout-menu-expanded');
    },

    renderChart: function (elementId, options) {
        if (typeof ApexCharts !== 'undefined') {
            var el = document.getElementById(elementId);
            if (el) {
                // Destroy existing chart if any
                if (this.charts[elementId]) {
                    this.charts[elementId].destroy();
                }

                var chart = new ApexCharts(el, options);
                chart.render();
                this.charts[elementId] = chart;
            }
        }
    },
    
    updateChart: function (elementId, seriesData) {
        if (this.charts[elementId]) {
            this.charts[elementId].updateSeries([{
                data: seriesData
            }]);
        }
    },

    appendDataPoint: function (elementId, dataPoint) {
         if (this.charts[elementId]) {
             // Use updateSeries to append or replace data
             // This assumes the chart series is structured correctly.
             // Simplest is to append data to existing series if manageable
             // Or user passes the full new series.
             // Here we assume full series update for simplicity unless specified
         }
    }
};
