import admin from './admin';
import components from './components';
import errors from './errors.js';
import site from 'site/i18n/ru.js';


export default {
  ...components,
  Admin: {...admin},
  Errors: {...errors},
  ...site
}
