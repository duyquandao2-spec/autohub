/**
 * AUTOHUB — Financial & Loan Calculator Script
 * Client-side reactive calculations for On-Road Cost and Bank Installments.
 */
(function () {
    'use strict';

    // Vehicles dataset injected from Razor Page
    const vehicles = window.AUTOHUB_VEHICLES || [];
    let currentCar = null;

    // State
    const state = {
        carPrice: 0,
        customPrice: false,
        city: 'hanoi',
        taxRate: 12,
        plateFee: 20000000,
        roadMonths: 12,
        roadFee: 1560000,
        inspectionFee: 340000,
        mandatoryInsurance: 480700,
        includePhysicalInsurance: true,
        physicalInsuranceRate: 0.015,

        // Results On-Road
        registrationTax: 0,
        physicalInsurance: 0,
        totalOnRoad: 0,

        // Loan
        loanBaseMode: 'car', // 'car' or 'onroad'
        downpaymentPct: 20,
        loanMonths: 60,
        interestRate: 8.5,
        method: 'reducing', // 'reducing' or 'fixed'

        // Results Loan
        downpaymentCash: 0,
        loanPrincipal: 0,
        monthlyPrincipal: 0,
        firstMonthPayment: 0,
        totalInterest: 0,
        initialCashRequired: 0
    };

    // Helper: format VND
    function formatVND(amount) {
        if (isNaN(amount) || amount === null) return '0 đ';
        return Math.round(amount).toLocaleString('vi-VN') + ' đ';
    }

    // DOM Elements
    const elements = {
        tabs: document.querySelectorAll('.finance-tab-btn'),
        panes: document.querySelectorAll('.finance-tab-pane'),

        carSelect: document.getElementById('fin-car-select'),
        customPriceCheck: document.getElementById('fin-custom-price-check'),
        customPriceInput: document.getElementById('fin-custom-price-input'),

        carImg: document.getElementById('fin-vehicle-img'),
        carBadge: document.getElementById('fin-vehicle-badge'),
        carName: document.getElementById('fin-vehicle-name'),
        carPrice: document.getElementById('fin-vehicle-price'),

        // On-Road elements
        cityPills: document.querySelectorAll('#fin-city-pills .fin-pill'),
        evNotice: document.getElementById('ev-tax-incentive-notice'),
        roadFeePills: document.querySelectorAll('#fin-road-fee-pills .fin-pill'),
        physInsuranceCheck: document.getElementById('fin-opt-physical-insurance'),
        physInsuranceAmount: document.getElementById('fin-phys-insurance-amount'),

        bdBasePrice: document.getElementById('bd-base-price'),
        bdTaxRateSub: document.getElementById('bd-tax-rate-sub'),
        bdRegistrationTax: document.getElementById('bd-registration-tax'),
        bdPlateFee: document.getElementById('bd-plate-fee'),
        bdInspectionFee: document.getElementById('bd-inspection-fee'),
        bdRoadFee: document.getElementById('bd-road-fee'),
        bdMandatoryInsurance: document.getElementById('bd-mandatory-insurance'),
        bdPhysicalInsuranceRow: document.getElementById('bd-physical-insurance-row'),
        bdPhysicalInsurance: document.getElementById('bd-physical-insurance'),
        bdTotalOnRoad: document.getElementById('bd-total-onroad'),

        btnSyncToLoan: document.getElementById('btn-sync-to-loan'),

        // Loan elements
        loanBasePills: document.querySelectorAll('#fin-loan-base-pills .fin-pill'),
        loanBaseAmount: document.getElementById('fin-loan-base-amount'),

        downpaymentSlider: document.getElementById('downpayment-slider'),
        downpaymentDisplay: document.getElementById('downpayment-percent-display'),
        downpaymentPills: document.querySelectorAll('#downpayment-pills .fin-pill'),
        downpaymentCashDisplay: document.getElementById('downpayment-cash-display'),
        loanAmountDisplay: document.getElementById('loan-amount-display'),

        loanTermSlider: document.getElementById('loan-term-slider'),
        loanTermDisplay: document.getElementById('loan-term-display'),
        loanTermMonthsDisplay: document.getElementById('loan-term-months-display'),
        loanTermPills: document.querySelectorAll('#loan-term-pills .fin-pill'),

        interestSlider: document.getElementById('interest-rate-slider'),
        interestDisplay: document.getElementById('interest-rate-display'),

        calcMethodRadios: document.querySelectorAll('input[name="calc-method"]'),

        kpiMonthlyFirst: document.getElementById('kpi-monthly-first'),
        kpiLoanPrincipal: document.getElementById('kpi-loan-principal'),
        kpiMonthlyPrincipal: document.getElementById('kpi-monthly-principal'),
        kpiTotalInterest: document.getElementById('kpi-total-interest'),
        kpiInitialCash: document.getElementById('kpi-initial-cash'),

        btnToggleSchedule: document.getElementById('btn-toggle-schedule'),
        scheduleWrapper: document.getElementById('amortization-table-wrapper'),
        scheduleTbody: document.getElementById('amortization-tbody'),

        // Hidden Form Inputs for Lead
        formSlug: document.getElementById('form-vehicle-slug'),
        formName: document.getElementById('form-vehicle-name'),
        formPrice: document.getElementById('form-vehicle-price'),
        formCity: document.getElementById('form-city'),
        formOnRoadCost: document.getElementById('form-onroad-cost'),
        formDownpaymentPct: document.getElementById('form-downpayment-pct'),
        formLoanAmount: document.getElementById('form-loan-amount'),
        formLoanTerm: document.getElementById('form-loan-term'),
        formInterestRate: document.getElementById('form-interest-rate'),
        formMonthlyFirst: document.getElementById('form-monthly-first')
    };

    // Initialize car
    function initCar() {
        const initialSlug = window.AUTOHUB_INITIAL_SLUG || (vehicles.length > 0 ? vehicles[0].slug : '');
        const found = vehicles.find(v => v.slug.toLowerCase() === initialSlug.toLowerCase());
        setCar(found || vehicles[0]);
    }

    function setCar(car) {
        if (!car) return;
        currentCar = car;

        if (elements.carSelect) {
            elements.carSelect.value = car.slug;
        }

        if (elements.carImg) {
            elements.carImg.src = car.image || '/Images/placeholder-car.jpg';
            elements.carImg.alt = car.name;
        }

        if (elements.carBadge) {
            elements.carBadge.textContent = `${car.category || 'XE SANG'} · ${car.fuel || 'XĂNG'}`;
        }

        if (elements.carName) {
            elements.carName.textContent = car.name;
        }

        if (!state.customPrice) {
            state.carPrice = Number(car.price) || 0;
            if (elements.carPrice) {
                elements.carPrice.textContent = car.priceFormatted || formatVND(state.carPrice);
            }
        }

        // Check if EV
        const isEv = (car.fuel || '').toUpperCase() === 'EV' || (car.fuel || '').toUpperCase() === 'ĐIỆN';
        if (elements.evNotice) {
            elements.evNotice.style.display = isEv ? 'block' : 'none';
        }

        // Adjust mandatory insurance according to seats
        const seats = car.seats || 5;
        state.mandatoryInsurance = seats > 5 ? 873400 : 480700;
        if (elements.bdMandatoryInsurance) {
            elements.bdMandatoryInsurance.textContent = formatVND(state.mandatoryInsurance);
        }

        calculateAll();
    }

    // Calculate On-Road Costs
    function calculateOnRoad() {
        const isEv = currentCar && ((currentCar.fuel || '').toUpperCase() === 'EV' || (currentCar.fuel || '').toUpperCase() === 'ĐIỆN');

        // Tax rate (0% for EV under decree 10/2022)
        const effectiveTaxRate = isEv ? 0 : state.taxRate;
        state.registrationTax = state.carPrice * (effectiveTaxRate / 100);

        // Physical vehicle insurance (1.5% of car base price)
        state.physicalInsurance = state.includePhysicalInsurance ? state.carPrice * state.physicalInsuranceRate : 0;

        // Total
        state.totalOnRoad = state.carPrice
            + state.registrationTax
            + state.plateFee
            + state.inspectionFee
            + state.roadFee
            + state.mandatoryInsurance
            + state.physicalInsurance;

        // Render On-Road breakdown
        if (elements.bdBasePrice) elements.bdBasePrice.textContent = formatVND(state.carPrice);
        if (elements.bdTaxRateSub) elements.bdTaxRateSub.textContent = isEv ? '(0% - Xe điện)' : `(${state.taxRate}%)`;
        if (elements.bdRegistrationTax) elements.bdRegistrationTax.textContent = formatVND(state.registrationTax);
        if (elements.bdPlateFee) elements.bdPlateFee.textContent = formatVND(state.plateFee);
        if (elements.bdInspectionFee) elements.bdInspectionFee.textContent = formatVND(state.inspectionFee);
        if (elements.bdRoadFee) elements.bdRoadFee.textContent = formatVND(state.roadFee);
        if (elements.bdPhysicalInsurance) elements.bdPhysicalInsurance.textContent = formatVND(state.physicalInsurance);
        if (elements.physInsuranceAmount) elements.physInsuranceAmount.textContent = formatVND(state.carPrice * state.physicalInsuranceRate);

        if (elements.bdPhysicalInsuranceRow) {
            elements.bdPhysicalInsuranceRow.style.display = state.includePhysicalInsurance ? 'flex' : 'none';
        }

        if (elements.bdTotalOnRoad) elements.bdTotalOnRoad.textContent = formatVND(state.totalOnRoad);
    }

    // Calculate Loan & Installment
    function calculateLoan() {
        // Base calculation price
        const baseAmount = state.loanBaseMode === 'onroad' ? state.totalOnRoad : state.carPrice;
        if (elements.loanBaseAmount) elements.loanBaseAmount.textContent = formatVND(baseAmount);

        state.downpaymentCash = baseAmount * (state.downpaymentPct / 100);
        state.loanPrincipal = baseAmount - state.downpaymentCash;

        const months = state.loanMonths;
        state.monthlyPrincipal = months > 0 ? (state.loanPrincipal / months) : 0;

        const monthlyRate = (state.interestRate / 100) / 12;

        if (state.method === 'reducing') {
            // First month interest = Principal * monthlyRate
            const firstMonthInterest = state.loanPrincipal * monthlyRate;
            state.firstMonthPayment = state.monthlyPrincipal + firstMonthInterest;

            // Total interest in reducing balance:
            // sum of remaining * monthlyRate for all months
            let totalInt = 0;
            let currentRemaining = state.loanPrincipal;
            for (let i = 1; i <= months; i++) {
                totalInt += currentRemaining * monthlyRate;
                currentRemaining -= state.monthlyPrincipal;
            }
            state.totalInterest = totalInt;
        } else {
            // Fixed balance
            const monthlyInterest = state.loanPrincipal * monthlyRate;
            state.firstMonthPayment = state.monthlyPrincipal + monthlyInterest;
            state.totalInterest = monthlyInterest * months;
        }

        // Initial cash required
        if (state.loanBaseMode === 'onroad') {
            state.initialCashRequired = state.downpaymentCash;
        } else {
            const onRoadFees = state.totalOnRoad - state.carPrice;
            state.initialCashRequired = state.downpaymentCash + onRoadFees;
        }

        // Render Loan KPIs
        if (elements.downpaymentDisplay) elements.downpaymentDisplay.textContent = state.downpaymentPct;
        if (elements.downpaymentCashDisplay) elements.downpaymentCashDisplay.textContent = formatVND(state.downpaymentCash);
        if (elements.loanAmountDisplay) elements.loanAmountDisplay.textContent = formatVND(state.loanPrincipal);

        if (elements.loanTermDisplay) elements.loanTermDisplay.textContent = Math.round(months / 12);
        if (elements.loanTermMonthsDisplay) elements.loanTermMonthsDisplay.textContent = months;
        if (elements.interestDisplay) elements.interestDisplay.textContent = state.interestRate.toFixed(1);

        if (elements.kpiMonthlyFirst) elements.kpiMonthlyFirst.textContent = formatVND(state.firstMonthPayment);
        if (elements.kpiLoanPrincipal) elements.kpiLoanPrincipal.textContent = formatVND(state.loanPrincipal);
        if (elements.kpiMonthlyPrincipal) elements.kpiMonthlyPrincipal.textContent = formatVND(state.monthlyPrincipal);
        if (elements.kpiTotalInterest) elements.kpiTotalInterest.textContent = formatVND(state.totalInterest);
        if (elements.kpiInitialCash) elements.kpiInitialCash.textContent = formatVND(state.initialCashRequired);

        renderAmortizationSchedule();
        syncFormData();
    }

    // Render Amortization schedule table
    function renderAmortizationSchedule() {
        if (!elements.scheduleTbody) return;
        elements.scheduleTbody.innerHTML = '';

        const months = state.loanMonths;
        const monthlyRate = (state.interestRate / 100) / 12;
        let remaining = state.loanPrincipal;

        const maxDisplayMonths = Math.min(months, 36); // Display up to 36 months, then summary

        for (let i = 1; i <= maxDisplayMonths; i++) {
            const interest = state.method === 'reducing' ? remaining * monthlyRate : state.loanPrincipal * monthlyRate;
            const principal = state.monthlyPrincipal;
            const totalMonth = principal + interest;
            const endRemaining = Math.max(0, remaining - principal);

            const tr = document.createElement('tr');
            tr.innerHTML = `
                <td>Tháng ${i}</td>
                <td>${formatVND(remaining)}</td>
                <td>${formatVND(principal)}</td>
                <td>${formatVND(interest)}</td>
                <td style="color: #ff4d4f; font-weight: 700;">${formatVND(totalMonth)}</td>
                <td>${formatVND(endRemaining)}</td>
            `;
            elements.scheduleTbody.appendChild(tr);

            remaining = endRemaining;
        }

        if (months > 36) {
            const tr = document.createElement('tr');
            tr.innerHTML = `
                <td colspan="6" style="text-align: center; color: #94a3b8; font-style: italic; padding: 1rem;">
                    ... và tiếp tục cho đến tháng ${months} (Dư nợ về 0 đ). Liên hệ AUTOHUB để nhận file Excel toàn kỳ chi tiết.
                </td>
            `;
            elements.scheduleTbody.appendChild(tr);
        }
    }

    // Sync calculated values to lead capture form
    function syncFormData() {
        if (!currentCar) return;
        if (elements.formSlug) elements.formSlug.value = currentCar.slug;
        if (elements.formName) elements.formName.value = currentCar.name;
        if (elements.formPrice) elements.formPrice.value = formatVND(state.carPrice);
        if (elements.formCity) elements.formCity.value = getCityName(state.city);
        if (elements.formOnRoadCost) elements.formOnRoadCost.value = formatVND(state.totalOnRoad);
        if (elements.formDownpaymentPct) elements.formDownpaymentPct.value = state.downpaymentPct.toString();
        if (elements.formLoanAmount) elements.formLoanAmount.value = formatVND(state.loanPrincipal);
        if (elements.formLoanTerm) elements.formLoanTerm.value = state.loanMonths.toString();
        if (elements.formInterestRate) elements.formInterestRate.value = state.interestRate.toFixed(1);
        if (elements.formMonthlyFirst) elements.formMonthlyFirst.value = formatVND(state.firstMonthPayment);
    }

    function getCityName(cityCode) {
        switch (cityCode) {
            case 'hanoi': return 'Hà Nội';
            case 'hcm': return 'TP. Hồ Chí Minh';
            case 'tier1': return 'Hải Phòng / Đà Nẵng / Quảng Ninh';
            default: return 'Tỉnh / Thành khác';
        }
    }

    function calculateAll() {
        calculateOnRoad();
        calculateLoan();
    }

    // Attach Event Listeners
    function attachEvents() {
        // Tab switching
        elements.tabs.forEach(tab => {
            tab.addEventListener('click', () => {
                const target = tab.dataset.tabTarget;
                elements.tabs.forEach(t => {
                    t.classList.remove('is-active');
                    t.setAttribute('aria-selected', 'false');
                });
                elements.panes.forEach(p => p.classList.remove('is-active'));

                tab.classList.add('is-active');
                tab.setAttribute('aria-selected', 'true');
                const activePane = document.querySelector(target);
                if (activePane) activePane.classList.add('is-active');
            });
        });

        // Vehicle select
        if (elements.carSelect) {
            elements.carSelect.addEventListener('change', e => {
                const slug = e.target.value;
                const found = vehicles.find(v => v.slug === slug);
                if (found) setCar(found);
            });
        }

        // Custom price checkbox & input
        if (elements.customPriceCheck && elements.customPriceInput) {
            elements.customPriceCheck.addEventListener('change', e => {
                state.customPrice = e.target.checked;
                elements.customPriceInput.style.display = state.customPrice ? 'block' : 'none';
                if (state.customPrice) {
                    elements.customPriceInput.value = state.carPrice;
                    elements.customPriceInput.focus();
                } else if (currentCar) {
                    state.carPrice = Number(currentCar.price) || 0;
                    if (elements.carPrice) elements.carPrice.textContent = currentCar.priceFormatted;
                    calculateAll();
                }
            });

            elements.customPriceInput.addEventListener('input', e => {
                const val = parseFloat(e.target.value);
                if (!isNaN(val) && val >= 0) {
                    state.carPrice = val;
                    if (elements.carPrice) elements.carPrice.textContent = formatVND(val);
                    calculateAll();
                }
            });
        }

        // City pills
        elements.cityPills.forEach(pill => {
            pill.addEventListener('click', () => {
                elements.cityPills.forEach(p => p.classList.remove('is-active'));
                pill.classList.add('is-active');
                state.city = pill.dataset.city;
                state.taxRate = parseFloat(pill.dataset.tax);
                state.plateFee = parseFloat(pill.dataset.plate);
                calculateAll();
            });
        });

        // Road fee pills
        elements.roadFeePills.forEach(pill => {
            pill.addEventListener('click', () => {
                elements.roadFeePills.forEach(p => p.classList.remove('is-active'));
                pill.classList.add('is-active');
                state.roadMonths = parseInt(pill.dataset.roadMonths, 10);
                state.roadFee = (state.roadMonths / 12) * 1560000;
                calculateAll();
            });
        });

        // Physical insurance checkbox
        if (elements.physInsuranceCheck) {
            elements.physInsuranceCheck.addEventListener('change', e => {
                state.includePhysicalInsurance = e.target.checked;
                calculateAll();
            });
        }

        // Sync to loan button
        if (elements.btnSyncToLoan) {
            elements.btnSyncToLoan.addEventListener('click', () => {
                // Switch to installment tab
                const installmentTab = document.querySelector('[data-tab-target="#tab-installment"]');
                if (installmentTab) installmentTab.click();

                // Select onroad mode
                elements.loanBasePills.forEach(p => {
                    p.classList.toggle('is-active', p.dataset.loanMode === 'onroad');
                });
                state.loanBaseMode = 'onroad';
                calculateLoan();

                // Smooth scroll down to panel
                const pane = document.getElementById('tab-installment');
                if (pane) pane.scrollIntoView({ behavior: 'smooth' });
            });
        }

        // Loan base pills
        elements.loanBasePills.forEach(pill => {
            pill.addEventListener('click', () => {
                elements.loanBasePills.forEach(p => p.classList.remove('is-active'));
                pill.classList.add('is-active');
                state.loanBaseMode = pill.dataset.loanMode;
                calculateLoan();
            });
        });

        // Downpayment slider & pills
        if (elements.downpaymentSlider) {
            elements.downpaymentSlider.addEventListener('input', e => {
                state.downpaymentPct = parseInt(e.target.value, 10);
                elements.downpaymentPills.forEach(p => {
                    p.classList.toggle('is-active', parseInt(p.dataset.pct, 10) === state.downpaymentPct);
                });
                calculateLoan();
            });
        }

        elements.downpaymentPills.forEach(pill => {
            pill.addEventListener('click', () => {
                elements.downpaymentPills.forEach(p => p.classList.remove('is-active'));
                pill.classList.add('is-active');
                state.downpaymentPct = parseInt(pill.dataset.pct, 10);
                if (elements.downpaymentSlider) elements.downpaymentSlider.value = state.downpaymentPct;
                calculateLoan();
            });
        });

        // Loan term slider & pills
        if (elements.loanTermSlider) {
            elements.loanTermSlider.addEventListener('input', e => {
                state.loanMonths = parseInt(e.target.value, 10);
                elements.loanTermPills.forEach(p => {
                    p.classList.toggle('is-active', parseInt(p.dataset.months, 10) === state.loanMonths);
                });
                calculateLoan();
            });
        }

        elements.loanTermPills.forEach(pill => {
            pill.addEventListener('click', () => {
                elements.loanTermPills.forEach(p => p.classList.remove('is-active'));
                pill.classList.add('is-active');
                state.loanMonths = parseInt(pill.dataset.months, 10);
                if (elements.loanTermSlider) elements.loanTermSlider.value = state.loanMonths;
                calculateLoan();
            });
        });

        // Interest slider
        if (elements.interestSlider) {
            elements.interestSlider.addEventListener('input', e => {
                state.interestRate = parseFloat(e.target.value);
                calculateLoan();
            });
        }

        // Method radios
        elements.calcMethodRadios.forEach(radio => {
            radio.addEventListener('change', e => {
                if (e.target.checked) {
                    state.method = e.target.value;
                    elements.calcMethodRadios.forEach(r => {
                        const card = r.closest('.radio-card');
                        if (card) card.classList.toggle('is-active', r.checked);
                    });
                    calculateLoan();
                }
            });
        });

        // Toggle schedule
        if (elements.btnToggleSchedule && elements.scheduleWrapper) {
            elements.btnToggleSchedule.addEventListener('click', () => {
                const isHidden = elements.scheduleWrapper.style.display === 'none';
                elements.scheduleWrapper.style.display = isHidden ? 'block' : 'none';
                elements.btnToggleSchedule.textContent = isHidden
                    ? 'Thu gọn bảng lịch trả nợ ↑'
                    : 'Xem lịch trả nợ chi tiết từng kỳ ↓';
            });
        }
    }

    // Startup
    document.addEventListener('DOMContentLoaded', () => {
        initCar();
        attachEvents();
    });

})();
