//npx tailwind build styles.css -o output.css

const path = require('path');
const process = require('process');
const colors = require('tailwindcss/colors');
const defaultTheme = require('tailwindcss/defaultTheme');
const generatePalette = require(path.resolve(__dirname, ('src/@fuse/tailwind/utils/generate-palette')));

/**
 * Custom palettes
 *
 * Uses the generatePalette helper method to generate
 * Tailwind-like color palettes automatically
 */
const customPalettes = {
    brand: generatePalette('#2196F3')
};

/**
 * Themes
 */
const themes = {
    // Default theme is required for theming system to work correctly
    'default': {
        primary: {
            ...colors.indigo,
            DEFAULT: colors.indigo[600]
        },
        accent: {
            ...colors.blueGray,
            DEFAULT: colors.blueGray[800]
        },
        warn: {
            ...colors.red,
            DEFAULT: colors.red[600]
        },
        'on-warn': {
            500: colors.red['50']
        }
    },/*
    // Rest of the themes will use the 'default' as the base theme
    // and extend them with their given configuration
    'brand': {
        primary: customPalettes.brand
    },
    'indigo': {
        primary: {
            ...colors.teal,
            DEFAULT: colors.teal[600]
        }
    },
    'rose': {
        primary: colors.rose
    },
    'purple': {
        primary: {
            ...colors.purple,
            DEFAULT: colors.purple[600]
        }
    },
    'amber': {
        primary: colors.amber
    }*/
};

/**
 * Tailwind configuration
 *
 * @param isProd
 * This will be automatically supplied by the custom Angular builder
 * based on the current environment of the application (prod, dev etc.)
 */
const config = {
    experimental: {},
    future: {},
    darkMode: 'class',
    important: true,
    purge: {
        enabled: true,
        content: ['./src/**/*.{html,scss,ts}'],
        options: {

            
            safelist: {
                standard: [/^text-/], //^bg-/,  /^px-/, /^py-/, /^rounded-/, /^grid-/, /^ring-/, /^border-/, /^flex-/, /^items-/, /^justify-/, /^flag-/, /^auto-/, /^invisible/, /^aspect-/, /^space-/, /^object-/
                deep: [] ///^theme/, /^dark/, /^mat/
            }
        }
    },
    theme: {
        colors: {
            transparent: 'transparent',
            current: 'currentColor',
            black: colors.black,
            white: colors.white,
            pink: colors.pink,
            gray: colors.blueGray,
            red: colors.red,
            orange: colors.orange,
            amber: colors.amber,
            yellow: colors.yellow,
            green: colors.green,
            teal: colors.teal,
            blue: colors.blue,
            indigo: colors.indigo,
            purple: colors.purple
        },
        fontSize: {
            'xs': '0.625rem',
            'sm': '0.75rem',
            'md': '0.8125rem',
            'base': '0.875rem',
            'lg': '1rem',
            'xl': '1.125rem',
            '2xl': '1.25rem',
            '3xl': '1.5rem',
            '4xl': '2rem',
            '5xl': '2.25rem',
            '6xl': '2.5rem',
            '7xl': '3rem',
            '8xl': '4rem',
            '9xl': '6rem',
            '10xl': '8rem'
        },
        screens: {
            //print: { 'raw': 'print' },
            sm: '640px',
            md: '768px',
            lg: '1024px',
            //xl: '1440px'
        },
        extend: {
            animation: {
                'spin-slow': 'spin 3s linear infinite'
            },
            flex: {
                '0': '0 0 auto'
            },
            fontFamily: {
                sans: ['Inter', ...defaultTheme.fontFamily.sans],
            },
            opacity: {
                12: '0.12',
                38: '0.38',
                87: '0.87'
            },
            rotate: {
                '-270': '270deg',
                '15': '15deg',
                '30': '30deg',
                '60': '60deg',
                '270': '270deg'
            },
            scale: {
                '-1': '-1'
            },
            zIndex: {
                '-1': -1,
                '49': 49,
                '60': 60,
                '70': 70,
                '80': 80,
                '90': 90,
                '99': 99,
                '999': 999,
                '9999': 9999,
                '99999': 99999
            },
            spacing: {
                '13': '3.25rem',
                '15': '3.75rem',
                '18': '4.5rem',
                '22': '5.5rem',
                '26': '6.5rem',
                '30': '7.5rem',
                '50': '12.5rem',
                '90': '22.5rem'
            },
            /**
             * Extended spacing values for width and height utilities.
             * This way, we won't be adding these to other utilities
             * that use 'spacing' config to keep the file size
             * smaller by not generating useless utilities such as
             * p-1/4 or m-480.
             */
            extendedSpacing: {
                // Fractional values
                '1/2': '50%',
                '1/3': '33.333333%',
                '2/3': '66.666667%',
                '1/4': '25%',
                '2/4': '50%',
                '3/4': '75%',
                '1/5': '20%',
                '2/5': '40%',
                '3/5': '60%',
                '4/5': '80%',
                '1/6': '16.666667%',
                '2/6': '33.333333%',
                '3/6': '50%',
                '4/6': '66.666667%',
                '5/6': '83.333333%',
                '1/12': '8.333333%',
                '2/12': '16.666667%',
                '3/12': '25%',
                '4/12': '33.333333%',
                '5/12': '41.666667%',
                '6/12': '50%',
                '7/12': '58.333333%',
                '8/12': '66.666667%',
                '9/12': '75%',
                '10/12': '83.333333%',
                '11/12': '91.666667%',

                // Bigger values
                '100': '25rem',
                '120': '30rem',
                '128': '32rem',
                '140': '35rem',
                '160': '40rem',
                '180': '45rem',
                '192': '48rem',
                '200': '50rem',
                '240': '60rem',
                '256': '64rem',
                '280': '70rem',
                '320': '80rem',
                '360': '90rem',
                '400': '100rem',
                '480': '120rem'
            },
            height: theme => ({
                ...theme('extendedSpacing')
            }),
            minHeight: theme => ({
                ...theme('spacing'),
                ...theme('extendedSpacing')
            }),
            maxHeight: theme => ({
                ...theme('extendedSpacing'),
                none: 'none'
            }),
            width: theme => ({
                ...theme('extendedSpacing')
            }),
            minWidth: theme => ({
                ...theme('spacing'),
                ...theme('extendedSpacing'),
                screen: '100vw'
            }),
            maxWidth: theme => ({
                ...theme('spacing'),
                ...theme('extendedSpacing'),
                screen: '100vw'
            }),

            // @tailwindcss/typography
            typography: (theme) => ({
                DEFAULT: {
                    css: {
                        color: 'var(--fuse-text-default)',
                        '[class~="lead"]': {
                            color: 'var(--fuse-text-secondary)'
                        },
                        a: {
                            color: 'var(--fuse-primary-500)'
                        },
                        strong: {
                            color: 'var(--fuse-text-default)'
                        },
                        'ol > li::before': {
                            color: 'var(--fuse-text-secondary)'
                        },
                        'ul > li::before': {
                            backgroundColor: 'var(--fuse-text-hint)'
                        },
                        hr: {
                            borderColor: 'var(--fuse-border)'
                        },
                        blockquote: {
                            color: 'var(--fuse-text-default)',
                            borderLeftColor: 'var(--fuse-border)'
                        },
                        h1: {
                            color: 'var(--fuse-text-default)'
                        },
                        h2: {
                            color: 'var(--fuse-text-default)'
                        },
                        h3: {
                            color: 'var(--fuse-text-default)'
                        },
                        h4: {
                            color: 'var(--fuse-text-default)'
                        },
                        'figure figcaption': {
                            color: 'var(--fuse-text-secondary)'
                        },
                        code: {
                            color: 'var(--fuse-text-default)',
                            fontWeight: '500'
                        },
                        'a code': {
                            color: 'var(--fuse-primary)'
                        },
                        pre: {
                            color: theme('colors.white'),
                            backgroundColor: theme('colors.gray.800')
                        },
                        thead: {
                            color: 'var(--fuse-text-default)',
                            borderBottomColor: 'var(--fuse-border)'
                        },
                        'tbody tr': {
                            borderBottomColor: 'var(--fuse-border)'
                        }
                    }
                },
                sm: {
                    css: {
                        code: {
                            fontSize: '1em'
                        },
                        pre: {
                            fontSize: '1em'
                        },
                        table: {
                            fontSize: '1em'
                        }
                    }
                }
            })
        }
    },
    variants: {
    },
    corePlugins: {
        appearance: false,
        gradientColorStops: true,
        container: false,
        float: false,
        clear: false,
        placeholderColor: false,
        placeholderOpacity: false,
        verticalAlign: false
    },
    plugins: [

        // Fuse - Tailwind plugins
        require(path.resolve(__dirname, ('src/@fuse/tailwind/plugins/extract-config'))),
        require(path.resolve(__dirname, ('src/@fuse/tailwind/plugins/utilities'))),
        require(path.resolve(__dirname, ('src/@fuse/tailwind/plugins/icon-size'))),
        require(path.resolve(__dirname, ('src/@fuse/tailwind/plugins/theming')))({ themes }),

        // Other third party and/or custom plugins
        require('@tailwindcss/typography')({ modifiers: ['sm', 'lg'] }),
        require('@tailwindcss/aspect-ratio'),
        require('@tailwindcss/line-clamp'),
        require('@tailwindcss/forms')
    ]
};

module.exports = config;
