import {
  afterNextRender,
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  effect,
  ElementRef,
  inject,
  input,
  viewChild,
} from '@angular/core';
import { Chart, ChartConfiguration, registerables } from 'chart.js';

// Register controllers/elements/scales once for the whole app.
Chart.register(...registerables);

/**
 * Thin, reusable Chart.js wrapper. Pass a `ChartConfiguration`; the component
 * (re)creates the chart whenever the config changes and cleans up on destroy.
 * Container sizing is the caller's responsibility (charts are non-aspect-ratio).
 */
@Component({
  selector: 'app-chart',
  template: '<canvas #canvas></canvas>',
  styles: [':host { display: block; position: relative; height: 100%; width: 100%; }'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ChartComponent {
  readonly config = input.required<ChartConfiguration>();

  private readonly canvas = viewChild.required<ElementRef<HTMLCanvasElement>>('canvas');
  private chart?: Chart;

  constructor() {
    // Build/rebuild only after the canvas exists in the DOM.
    afterNextRender(() => this.render());

    effect(() => {
      this.config(); // track config changes
      if (this.chart) {
        this.render();
      }
    });

    inject(DestroyRef).onDestroy(() => this.chart?.destroy());
  }

  private render(): void {
    this.chart?.destroy();
    this.chart = new Chart(this.canvas().nativeElement, this.config());
  }
}
