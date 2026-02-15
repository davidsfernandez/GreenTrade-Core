window.appInterop = {
    // Store instances globally
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

    // TradingView Lightweight Charts
    createTradingViewChart: function (elementId, initialData) {
        const container = document.getElementById(elementId);
        if (!container || typeof LightweightCharts === 'undefined') return;

        if (this.instances[elementId]) {
            if (this.instances[elementId].remove) this.instances[elementId].remove();
            delete this.instances[elementId];
        }
        
        container.innerHTML = '';

        try {
            const chart = LightweightCharts.createChart(container, {
                width: container.clientWidth,
                height: 300,
                layout: {
                    backgroundColor: '#ffffff',
                    textColor: '#566a7f',
                    fontSize: 12,
                    fontFamily: 'Public Sans, sans-serif',
                },
                grid: {
                    vertLines: { color: '#eceef1' },
                    horzLines: { color: '#eceef1' },
                },
                rightPriceScale: { borderColor: '#d9d9d9' },
                timeScale: { borderColor: '#d9d9d9', timeVisible: true, secondsVisible: false },
            });

            // Main Price Series (Area)
            const mainSeries = chart.addAreaSeries({
                topColor: 'rgba(105, 108, 255, 0.56)',
                bottomColor: 'rgba(105, 108, 255, 0.04)',
                lineColor: 'rgba(105, 108, 255, 1)',
                lineWidth: 2,
            });

            if (initialData && initialData.length > 0) {
                initialData.sort((a, b) => a.time - b.time);
                mainSeries.setData(initialData);
            }

            // Store instance
            const resizeObserver = new ResizeObserver(entries => {
                if (entries.length === 0 || entries[0].target !== container) return;
                const newRect = entries[0].contentRect;
                chart.applyOptions({ width: newRect.width, height: newRect.height });
            });
            resizeObserver.observe(container);

            this.instances[elementId] = {
                chart: chart,
                mainSeries: mainSeries,
                indicators: {}, // Store indicator series by name
                resizeObserver: resizeObserver,
                remove: function() {
                    resizeObserver.disconnect();
                    chart.remove();
                }
            };
        } catch (e) {
            console.error('Error creating TradingView chart:', e);
        }
    },

    updateTradingViewChart: function (elementId, dataPoint) {
        const instance = this.instances[elementId];
        if (instance && instance.mainSeries) {
            instance.mainSeries.update(dataPoint);
        }
    },

    // Add generic line indicator (e.g., SMA)
    addIndicatorSeries: function (elementId, name, color, data) {
        const instance = this.instances[elementId];
        if (!instance) return;

        // Create if not exists
        if (!instance.indicators[name]) {
            instance.indicators[name] = instance.chart.addLineSeries({
                color: color,
                lineWidth: 1,
                priceLineVisible: false,
                lastValueVisible: false,
            });
        }

        if (data && data.length > 0) {
            data.sort((a, b) => a.time - b.time);
            instance.indicators[name].setData(data);
        }
    },

    updateIndicatorSeries: function (elementId, name, dataPoint) {
        const instance = this.instances[elementId];
        if (instance && instance.indicators[name]) {
            instance.indicators[name].update(dataPoint);
        }
    }
};
