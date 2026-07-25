import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

/**
 * Root shell. Deliberately thin — it only hosts the top-level router outlet.
 * Layout chrome (sidebar / toolbar) lives in the layout components so it can be
 * swapped per route (authenticated shell vs. bare auth screens).
 */
@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class App {}
