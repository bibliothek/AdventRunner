import { Store } from 'vuex'
import { State } from './store';

declare module 'vue' {
  // provide typings for `this.$store`
  interface ComponentCustomProperties {
    $store: Store<State>
  }
}