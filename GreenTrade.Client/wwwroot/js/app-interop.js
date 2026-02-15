window.appInterop = {
    instances: {},

    initializeSneatMenu: function () {
        if (typeof Menu !== 'undefined' && document.querySelector('#layout-menu')) {
            new Menu(document.querySelector('#layout-menu'), { orientation: 'vertical', closeChildren: false });
        }
        const menuInnerContainer = document.querySelector('.menu-inner');
        if (menuInnerContainer && typeof PerfectScrollbar !== 'undefined') {
            new PerfectScrollbar(menuInnerContainer, { wheelPropagation: false, suppressScrollX: true });
        }
    },

    toggleMobileMenu: function () {
        document.documentElement.classList.toggle('layout-menu-expanded');
    },

    setTheme: function (isDark) {
        if (isDark) {
            document.documentElement.classList.remove('light-style');
            document.documentElement.classList.add('dark-style');
            localStorage.setItem('theme', 'dark');
        } else {
            document.documentElement.classList.remove('dark-style');
            document.documentElement.classList.add('light-style');
            localStorage.setItem('theme', 'light');
        }
    },

    getTheme: function () {
        return localStorage.getItem('theme') || 'light';
    },

    // --- TradingView Logic ---

    createTradingViewChart: function (elementId, initialData) {
        const container = document.getElementById(elementId);
        if (!container || typeof LightweightCharts === 'undefined') return;

        this._cleanupInstance(elementId);
        container.innerHTML = '';

        try {
            const chart = LightweightCharts.createChart(container, {
                width: container.clientWidth,
                height: 300,
                layout: { backgroundColor: '#ffffff', textColor: '#566a7f', fontSize: 12, fontFamily: 'Public Sans' },
                grid: { vertLines: { color: '#eceef1' }, horzLines: { color: '#eceef1' } },
                rightPriceScale: { borderColor: '#d9d9d9' },
                timeScale: { borderColor: '#d9d9d9', timeVisible: true, secondsVisible: false },
            });

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

            this._storeInstance(elementId, container, chart, mainSeries);
        } catch (e) {
            console.error('Error creating Main Chart:', e);
        }
    },

    createRsiChart: function (elementId, initialData) {
        const container = document.getElementById(elementId);
        if (!container || typeof LightweightCharts === 'undefined') return;

        this._cleanupInstance(elementId);
        container.innerHTML = '';

        try {
            const chart = LightweightCharts.createChart(container, {
                width: container.clientWidth,
                height: 150,
                layout: { backgroundColor: '#ffffff', textColor: '#566a7f', fontSize: 12, fontFamily: 'Public Sans' },
                grid: { vertLines: { color: '#eceef1' }, horzLines: { color: '#eceef1' } },
                rightPriceScale: { borderColor: '#d9d9d9' },
                timeScale: { borderColor: '#d9d9d9', timeVisible: true, secondsVisible: false },
            });

            // RSI Line
            const rsiSeries = chart.addLineSeries({
                color: '#696cff',
                lineWidth: 2,
            });

            // Reference Lines (70 and 30) - Lightweight Charts 4.x creates generic PriceLines for specific series
            // But we can just rely on grid or custom primitive if needed. 
            // For now, simpler approach: no explicit horizontal lines unless we add extra series or primitives.
            // Actually, createPriceLine is available on series.
            rsiSeries.createPriceLine({ price: 70, color: '#ff5b5c', lineWidth: 1, lineStyle: 2, axisLabelVisible: false, title: 'Overbought' });
            rsiSeries.createPriceLine({ price: 30, color: '#71dd37', lineWidth: 1, lineStyle: 2, axisLabelVisible: false, title: 'Oversold' });

            if (initialData && initialData.length > 0) {
                initialData.sort((a, b) => a.time - b.time);
                rsiSeries.setData(initialData);
            }

            this._storeInstance(elementId, container, chart, rsiSeries);
        } catch (e) {
            console.error('Error creating RSI Chart:', e);
        }
    },

    syncCharts: function (mainId, rsiId) {
        const main = this.instances[mainId];
        const rsi = this.instances[rsiId];

        if (!main || !rsi) return;

        // Sync TimeScales
        // Using subscribeVisibleLogicalRangeChange to sync scrolling
        main.chart.timeScale().subscribeVisibleLogicalRangeChange(range => {
            if (range) rsi.chart.timeScale().setVisibleLogicalRange(range);
        });

        rsi.chart.timeScale().subscribeVisibleLogicalRangeChange(range => {
            if (range) main.chart.timeScale().setVisibleLogicalRange(range);
        });
        
        // Sync Crosshair (basic) - requires more complex mouse event handling for full effect
        // For MVP, time sync is sufficient.
    },

    updateTradingViewChart: function (elementId, dataPoint) {
        const instance = this.instances[elementId];
        if (instance && instance.mainSeries) {
            instance.mainSeries.update(dataPoint);
        }
    },

    updateIndicatorSeries: function (elementId, name, dataPoint) {
        // For Overlay (SMA)
        const instance = this.instances[elementId];
        if (instance && instance.indicators && instance.indicators[name]) {
            instance.indicators[name].update(dataPoint);
        }
    },
    
    updateRsiSeries: function (elementId, dataPoint) {
        // For Separate Pane (RSI) - Reusing mainSeries property for convenience or checking instance type
        const instance = this.instances[elementId];
        if (instance && instance.mainSeries) {
            instance.mainSeries.update(dataPoint);
        }
    },

    addIndicatorSeries: function (elementId, name, color, data) {
        const instance = this.instances[elementId];
        if (!instance) return;
        if (!instance.indicators) instance.indicators = {};

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

    // Helper: Cleanup
    _cleanupInstance: function(id) {
        if (this.instances[id]) {
            if (this.instances[id].remove) this.instances[id].remove();
            delete this.instances[id];
        }
    },

    // Helper: Store
    _storeInstance: function(id, container, chart, mainSeries) {
        const resizeObserver = new ResizeObserver(entries => {
            if (entries.length === 0 || entries[0].target !== container) return;
            const newRect = entries[0].contentRect;
            chart.applyOptions({ width: newRect.width, height: newRect.height });
        });
        resizeObserver.observe(container);

        this.instances[id] = {
            chart: chart,
            mainSeries: mainSeries, // Points to Area for Main, Line for RSI
            indicators: {},
            resizeObserver: resizeObserver,
            remove: function() {
                resizeObserver.disconnect();
                chart.remove();
            }
        };
    }
};
