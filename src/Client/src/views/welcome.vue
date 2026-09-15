<script lang="js">
import {inject} from 'vue';

// Purely decorative sample used in the hero. Mirrors the real door states
// (done / open / closed) so the landing page always matches the app.
const heroDoors = [
  { day: 1, state: 'done', km: 5 },
  { day: 2, state: 'done', km: 3 },
  { day: 3, state: 'done', km: 8 },
  { day: 4, state: 'done', km: 4 },
  { day: 5, state: 'open', km: 10 },
  { day: 6, state: 'closed' },
  { day: 7, state: 'closed' },
  { day: 8, state: 'closed' },
  { day: 9, state: 'closed' },
];

const steps = [
  {
    icon: 'fa-solid fa-door-open',
    title: 'Open today’s door',
    text: 'Every day in December hides a distance. One tap and it’s revealed.',
  },
  {
    icon: 'fa-solid fa-person-running',
    title: 'Run the distance',
    text: 'Anywhere from a quick 1 km to a proper long one. No two Decembers are alike.',
  },
  {
    icon: 'fa-solid fa-check',
    title: 'Tick it off',
    text: 'Tap the door again and watch the bar fill up all the way to the 24th.',
  },
];

const features = [
  {
    icon: 'fa-solid fa-share-nodes',
    title: 'Share your month',
    text: 'Export your calendar as an image or hand out a live link friends can follow.',
  },
  {
    icon: 'fa-solid fa-sliders',
    title: 'Your distances',
    text: 'Halve them, double them, or take them as they come. The challenge is yours to size.',
  },
  {
    icon: 'fa-solid fa-table-cells-large',
    title: 'Two ways to look',
    text: 'Use the classic doors or switch to a month grid for the calendar.',
  },
];

export default {
  data() {
    return { heroDoors, steps, features };
  },
  inject: ["Auth"],
  methods: {
    login() {
      this.Auth.loginWithRedirect();
    },
    logout() {
        this.Auth.logout();
    },
    goToCalendar() {
      this.$router.push("calendar")
    },
    // The app runs on hash history, so an "#how-it-works" href would be read as
    // a route. Scroll manually instead.
    scrollToHowItWorks() {
      this.$refs.howItWorks?.scrollIntoView({ behavior: 'smooth', block: 'start' });
    },
  },


  setup() {
    const auth = inject("Auth");
    return {
      ...auth,
    };
  },
};
</script>
<template>
  <div>
    <!-- ---------------------------------------------------------- Hero -->
    <section
      class="relative -mx-3 sm:-mx-6 overflow-hidden bg-gradient-to-b from-ink-50 to-white px-5 py-10 sm:px-10 sm:py-14 lg:py-20"
    >
      <!-- Decorative glows -->
      <div aria-hidden="true"
           class="pointer-events-none absolute -top-28 -right-24 h-72 w-72 rounded-full bg-primary/20 blur-3xl"></div>
      <div aria-hidden="true"
           class="pointer-events-none absolute -bottom-40 -left-32 h-80 w-80 rounded-full bg-primary/10 blur-3xl"></div>

      <div class="relative grid gap-10 lg:grid-cols-2 lg:items-center lg:gap-14">
        <!-- Copy -->
        <div class="text-center lg:text-left animate-fade-up">
          <span class="pill bg-white text-ink-600 shadow-door text-[10px] sm:text-xs whitespace-normal">
            🎄 24 doors · 24 runs · one December
          </span>

          <h1 class="mt-5 text-4xl sm:text-5xl lg:text-6xl font-extrabold tracking-tight text-ink-800 leading-[1.05]">
            Open a door.<br />
            <span class="text-primary">Go for a run.</span>
          </h1>

          <p class="mt-4 text-base sm:text-lg text-ink-400 max-w-xl mx-auto lg:mx-0 leading-relaxed">
            Adventrunner turns December into an advent calendar of runs. Behind every
            door is a distance — open it, run it, and watch the month add up.
          </p>

          <div v-if="!loading" class="mt-7 animate-fade-up animation-delay-150">
            <div v-if="!isAuthenticated" class="flex flex-col sm:flex-row gap-3 sm:justify-center lg:justify-start">
              <button class="btn btn-primary btn-lg w-full sm:w-auto gap-2 shadow-door" @click="login">
                Start your calendar
                <font-awesome-icon icon="fa-solid fa-arrow-right" />
              </button>
              <button
                class="btn btn-lg w-full sm:w-auto bg-white text-ink-600 border border-ink-100 hover:bg-ink-50 hover:border-ink-100 shadow-door"
                @click="scrollToHowItWorks"
              >
                How it works
              </button>
            </div>
            <div v-else class="flex flex-col sm:flex-row gap-3 sm:justify-center lg:justify-start">
              <button class="btn btn-primary btn-lg w-full sm:w-auto gap-2 shadow-door" @click="goToCalendar">
                Go to your calendar
                <font-awesome-icon icon="fa-solid fa-arrow-right" />
              </button>
              <button class="btn btn-ghost btn-lg w-full sm:w-auto text-ink-600" @click="logout">
                Log out
              </button>
            </div>
          </div>

          <div class="mt-6 flex items-center justify-center lg:justify-start gap-2 text-sm text-ink-400">
            <span class="flex h-5 w-5 items-center justify-center rounded bg-strava shrink-0">
              <img class="w-3" src="/strava-icon.png" alt="" />
            </span>
            <span>Connect Strava and your December kilometres are verified automatically.</span>
          </div>
        </div>

        <!-- Door preview -->
        <div class="relative pb-16 animate-fade-up animation-delay-300" aria-hidden="true">
          <div class="mx-auto w-full max-w-sm lg:max-w-md">
            <div class="grid grid-cols-3 gap-2.5 sm:gap-4">
              <div
                v-for="door in heroDoors"
                :key="door.day"
                class="door"
                :class="{
                  'door-done': door.state === 'done',
                  'door-open': door.state === 'open',
                  'door-closed': door.state === 'closed',
                }"
              >
                <!-- Same panel elements the real doors use, so the hero keeps
                     matching the calendar. -->
                <template v-if="door.state === 'closed'">
                  <span class="door-panel door-panel-l"></span>
                  <span class="door-panel door-panel-r"></span>
                </template>
                <span v-else class="door-panel door-frame"></span>

                <span
                  v-if="door.state !== 'closed'"
                  class="absolute top-3 left-3.5 door-day text-[11px] sm:text-xs"
                  :class="door.state === 'done' ? 'text-white/70' : 'text-primary/60'"
                >{{ door.day }}</span>

                <span v-if="door.state === 'closed'" class="door-day text-3xl sm:text-4xl">{{ door.day }}</span>
                <span v-else class="door-distance text-lg sm:text-2xl">
                  {{ door.km }}<span class="text-[0.6em] font-semibold ml-0.5">km</span>
                </span>

                <span v-if="door.state === 'done'" class="mt-1 text-[11px] sm:text-sm">🎉</span>
              </div>
            </div>
          </div>

          <!-- Floating progress chip -->
          <div
            class="absolute bottom-0 left-1/2 -translate-x-1/2
                   bg-white rounded-2xl shadow-soft border border-ink-100 px-4 py-3 w-60 animate-float"
          >
            <div class="flex items-baseline justify-between">
              <span class="text-xs font-semibold uppercase tracking-widest text-ink-400">Progress</span>
              <span class="text-sm font-bold text-primary tnum">20 / 300 km</span>
            </div>
            <div class="mt-2 h-2 rounded-full bg-ink-100 overflow-hidden">
              <div class="h-full rounded-full bg-primary" style="width: 23%"></div>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- -------------------------------------------------- How it works -->
    <section id="how-it-works" ref="howItWorks" class="pt-16 sm:pt-20 scroll-mt-4">
      <div class="text-center max-w-2xl mx-auto">
        <span class="pill bg-primary/10 text-primary">How it works</span>
        <h2 class="mt-4 text-3xl sm:text-4xl font-bold tracking-tight text-ink-800">
          Three taps a day, all December long
        </h2>
      </div>

      <div class="mt-10 grid gap-4 sm:gap-6 md:grid-cols-3">
        <div
          v-for="(step, i) in steps"
          :key="step.title"
          class="relative rounded-3xl border border-ink-100 bg-ink-50 p-6 pt-8"
        >
          <span
            class="absolute -top-4 left-6 flex h-9 w-9 items-center justify-center rounded-xl
                   bg-primary text-primary-content font-bold shadow-door tnum"
          >{{ i + 1 }}</span>
          <font-awesome-icon :icon="step.icon" class="text-2xl text-primary" />
          <h3 class="mt-3 text-lg font-bold text-ink-800">{{ step.title }}</h3>
          <p class="mt-1.5 text-sm text-ink-400 leading-relaxed">{{ step.text }}</p>
        </div>
      </div>
    </section>

    <!-- ----------------------------------------------------- Features -->
    <section class="pt-16 sm:pt-20">
      <div class="text-center max-w-2xl mx-auto">
        <span class="pill bg-secondary/10 text-secondary">Along the way</span>
        <h2 class="mt-4 text-3xl sm:text-4xl font-bold tracking-tight text-ink-800">
          Built for people who like finishing things
        </h2>
      </div>

      <div class="mt-10 grid gap-4 sm:gap-5 sm:grid-cols-2 lg:grid-cols-3">
        <div
          v-for="feature in features"
          :key="feature.title"
          class="rounded-3xl border border-ink-100 p-5 transition-shadow hover:shadow-soft"
        >
          <span class="flex h-10 w-10 items-center justify-center rounded-xl bg-primary/10 text-primary">
            <font-awesome-icon :icon="feature.icon" class="text-lg" />
          </span>
          <h3 class="mt-3 font-bold text-ink-800">{{ feature.title }}</h3>
          <p class="mt-1.5 text-sm text-ink-400 leading-relaxed">{{ feature.text }}</p>
        </div>
      </div>
    </section>

    <!-- ---------------------------------------------------- Final CTA -->
    <section class="mt-16 sm:mt-20 -mx-3 sm:-mx-6">
      <div class="relative overflow-hidden bg-gradient-to-br from-primary to-[#3fb377] px-6 py-12 sm:px-12 sm:py-16 text-center">
        <div aria-hidden="true"
             class="pointer-events-none absolute -top-20 -left-16 h-64 w-64 rounded-full bg-white/10 blur-2xl"></div>
        <h2 class="relative text-3xl sm:text-4xl font-extrabold text-primary-content tracking-tight">
          December is waiting behind door number one
        </h2>
        <p class="relative mt-3 text-primary-content/85 max-w-lg mx-auto">
          Set up your calendar in under a minute. It’s free, and door 1 opens whenever you’re ready.
        </p>
        <div class="relative mt-7" v-if="!loading">
          <button
            v-if="!isAuthenticated"
            class="btn btn-lg bg-white text-primary hover:bg-white/90 border-none shadow-soft gap-2"
            @click="login"
          >
            Log in and start running
            <font-awesome-icon icon="fa-solid fa-arrow-right" />
          </button>
          <button
            v-else
            class="btn btn-lg bg-white text-primary hover:bg-white/90 border-none shadow-soft gap-2"
            @click="goToCalendar"
          >
            Open your calendar
            <font-awesome-icon icon="fa-solid fa-arrow-right" />
          </button>
        </div>
      </div>
    </section>

    <!-- -------------------------------------------------------- Footer -->
    <footer class="pt-8 pb-2 flex flex-col sm:flex-row items-center justify-center gap-3 text-sm text-ink-400">
      <span>adventrunner.com</span>
      <span class="hidden sm:inline">·</span>
      <img class="h-8" src="/powered-by-strava.png" alt="Powered by Strava" />
    </footer>
  </div>
</template>
