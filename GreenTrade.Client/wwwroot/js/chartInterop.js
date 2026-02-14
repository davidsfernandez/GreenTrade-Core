window.chartInterop = {
    charts: {},
    initChart: function (containerId, title) {
        const container = document.getElementById(containerId);
        if (!container) return;

        const chart = LightweightCharts.createChart(container, {
            width: container.offsetWidth,
            height: 300,
            layout: {
                backgroundColor: '#2c3e50',
                textColor: '#ecf0f1',
            },
            grid: {
                vertLines: { color: '#34495e' },
                horzLines: { color: '#34495e' },
            },
            timeScale: {
                timeVisible: true,
                secondsVisible: false,
            },
        });

        const lineSeries = chart.addLineSeries({
            color: '#2ecc71',
            lineWidth: 2,
            title: title
        });

        this.charts[containerId] = { chart, lineSeries, data: [] };

        window.addEventListener('resize', () => {
            chart.applyOptions({ width: container.offsetWidth });
        });
    },
    updateChart: function (containerId, time, value) {
        const entry = this.charts[containerId];
        if (!entry) return;

        const newData = { time: time, value: value };
        entry.data.push(newData);

        // Keep last 50 points
        if (entry.data.length > 50) {
            entry.data.shift();
        }

        entry.lineSeries.setData(entry.data);
    }
};
