window.appInterop = {
    // Store instances globally to manage updates and resizing
    instances: {},

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
        document.documentElement.classList.toggle('layout-menu-expanded');
    },

    // Legacy ApexCharts Support
    renderChart: function (elementId, options) {
        if (typeof ApexCharts !== 'undefined') {
            var el = document.getElementById(elementId);
            if (el) {
                if (this.instances[elementId] && this.instances[elementId].destroy) {
                    this.instances[elementId].destroy();
                }
                var chart = new ApexCharts(el, options);
                chart.render();
                this.instances[elementId] = chart;
            }
        }
    },
    
    updateChart: function (elementId, seriesData) {
        if (this.instances[elementId] && this.instances[elementId].updateSeries) {
            this.instances[elementId].updateSeries([{
                data: seriesData
            }]);
        }
    },

    // TradingView Lightweight Charts Implementation
    createTradingViewChart: function (elementId, initialData) {
        const container = document.getElementById(elementId);
        if (!container) return;

        // Cleanup existing instance if any
        if (this.instances[elementId]) {
            // ApexCharts destroy method
            if (this.instances[elementId].destroy) this.instances[elementId].destroy();
            // Lightweight Charts remove method
            if (this.instances[elementId].remove) this.instances[elementId].remove();
            
            // Remove resize observer if attached (custom logic needed if stored separately)
            delete this.instances[elementId];
        }
        
        container.innerHTML = ''; // Clear container

        const chart = LightweightCharts.createChart(container, {
            width: container.clientWidth,
            height: 300,
            layout: {
                backgroundColor: '#ffffff',
                textColor: '#566a7f', // Sneat text color
                fontSize: 12,
                fontFamily: 'Public Sans, sans-serif',
            },
            grid: {
                vertLines: { color: '#eceef1' },
                horzLines: { color: '#eceef1' },
            },
            rightPriceScale: {
                borderColor: '#d9d9d9',
            },
            timeScale: {
                borderColor: '#d9d9d9',
                timeVisible: true,
                secondsVisible: false,
            },
        });

        const areaSeries = chart.addAreaSeries({
            topColor: 'rgba(105, 108, 255, 0.56)', // Sneat Primary
            bottomColor: 'rgba(105, 108, 255, 0.04)',
            lineColor: 'rgba(105, 108, 255, 1)',
            lineWidth: 2,
        });

        if (initialData && initialData.length > 0) {
            // Ensure data is sorted by time. Data should be [{ time: '2019-04-11', value: 80.01 }]
            areaSeries.setData(initialData);
        }

        // Handle Resize
        const resizeObserver = new ResizeObserver(entries => {
            if (entries.length === 0 || entries[0].target !== container) { return; }
            const newRect = entries[0].contentRect;
            chart.applyOptions({ width: newRect.width, height: newRect.height });
        });
        resizeObserver.observe(container);

        // Store instance along with series for updates
        this.instances[elementId] = {
            chart: chart,
            series: areaSeries,
            resizeObserver: resizeObserver,
            remove: function() {
                resizeObserver.disconnect();
                chart.remove();
            }
        };
    },

    updateTradingViewChart: function (elementId, dataPoint) {
        // dataPoint format: { time: 'yyyy-mm-dd' or timestamp, value: 123.45 }
        const instance = this.instances[elementId];
        if (instance && instance.series) {
            instance.series.update(dataPoint);
        }
    }
};
